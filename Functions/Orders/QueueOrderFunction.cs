using System.Net;
using System.Text.Json;
using CoffeeAndChill.DTOs;
using CoffeeAndChill.Models;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions.Orders;

/// <summary>
/// Accepts an order, validates it and queues it immediately. Processing happens
/// asynchronously in ProcessOrderQueue, so the caller is not held up by the barista lifecycle.
/// </summary>
public sealed class QueueOrderFunction
{
    private readonly ILogger<QueueOrderFunction> _logger;

    public QueueOrderFunction(ILogger<QueueOrderFunction> logger) => _logger = logger;

    [Function("QueueOrder")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")] HttpRequestData request)
    {
        CreateOrderRequest? body;
        try
        {
            body = await JsonSerializer.DeserializeAsync<CreateOrderRequest>(request.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Order queue request contained malformed JSON.");
            return await WriteErrorAsync(request, HttpStatusCode.BadRequest, "The order body must be valid JSON.");
        }

        var errors = Validate(body);
        if (errors.Count > 0)
        {
            var invalid = request.CreateResponse(HttpStatusCode.BadRequest);
            await invalid.WriteAsJsonAsync(new { errors });
            return invalid;
        }

        var order = new Order
        {
            OrderId = body!.OrderId!.Trim(),
            CustomerName = body.CustomerName!.Trim(),
            SelectedItemSKUs = body.SelectedItemSKUs!.Select(sku => sku.Trim()).ToList(),
            TotalPrice = body.TotalPrice,
            OrderTimestamp = body.OrderTimestamp ?? DateTimeOffset.UtcNow
        };

        var queue = await OrderQueue.GetAsync();
        await queue.SendMessageAsync(JsonSerializer.Serialize(order));
        _logger.LogInformation("Order {OrderId} was queued for asynchronous processing.", order.OrderId);

        var accepted = request.CreateResponse(HttpStatusCode.Accepted);
        await accepted.WriteAsJsonAsync(new
        {
            orderId = order.OrderId,
            queue = OrderQueue.QueueName,
            statusUrl = $"/api/orders/{Uri.EscapeDataString(order.OrderId)}/status"
        });
        return accepted;
    }

    private static List<string> Validate(CreateOrderRequest? body)
    {
        var errors = new List<string>();
        if (body is null)
        {
            errors.Add("An order body is required.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(body.OrderId)) errors.Add("OrderId is required.");
        if (string.IsNullOrWhiteSpace(body.CustomerName)) errors.Add("CustomerName is required.");
        if (body.SelectedItemSKUs is null || body.SelectedItemSKUs.Count == 0 || body.SelectedItemSKUs.Any(string.IsNullOrWhiteSpace))
            errors.Add("SelectedItemSKUs must contain at least one SKU.");
        if (body.TotalPrice <= 0) errors.Add("TotalPrice must be greater than zero.");
        return errors;
    }

    private static async Task<HttpResponseData> WriteErrorAsync(HttpRequestData request, HttpStatusCode status, string error)
    {
        var response = request.CreateResponse(status);
        await response.WriteAsJsonAsync(new { error });
        return response;
    }
}
