using System.Net;
using System.Text.Json;
using CoffeeAndChill.Models;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions.Orders;

/// <summary>
/// Accepts an order and queues it immediately. Processing happens asynchronously in
/// <see cref="ProcessOrderQueue"/>, so the caller is not held up by the barista lifecycle.
/// </summary>
public sealed class QueueOrderFunction
{
    private readonly ILogger<QueueOrderFunction> _logger;

    public QueueOrderFunction(ILogger<QueueOrderFunction> logger) => _logger = logger;

    [Function("QueueOrder")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")] HttpRequestData request)
    {
        Order? order;
        try
        {
            order = await JsonSerializer.DeserializeAsync<Order>(request.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Order queue request contained malformed JSON.");
            return await WriteErrorAsync(request, HttpStatusCode.BadRequest, "The order body must be valid JSON.");
        }

        var errors = Validate(order);
        if (errors.Count > 0)
        {
            var invalid = request.CreateResponse(HttpStatusCode.BadRequest);
            await invalid.WriteAsJsonAsync(new { errors });
            return invalid;
        }

        var queue = await OrderQueue.GetAsync();
        var message = JsonSerializer.Serialize(order);
        await queue.SendMessageAsync(message);
        _logger.LogInformation("Order {OrderId} was queued for asynchronous processing.", order!.OrderId);

        var accepted = request.CreateResponse(HttpStatusCode.Accepted);
        await accepted.WriteAsJsonAsync(new
        {
            orderId = order.OrderId,
            queue = OrderQueue.QueueName,
            statusUrl = $"/api/orders/{Uri.EscapeDataString(order.OrderId)}/status"
        });
        return accepted;
    }

    private static List<string> Validate(Order? order)
    {
        var errors = new List<string>();
        if (order is null)
        {
            errors.Add("An order body is required.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(order.OrderId)) errors.Add("OrderId is required.");
        if (string.IsNullOrWhiteSpace(order.CustomerName)) errors.Add("CustomerName is required.");
        if (order.SelectedItemSKUs is null || order.SelectedItemSKUs.Count == 0 || order.SelectedItemSKUs.Any(string.IsNullOrWhiteSpace))
            errors.Add("SelectedItemSKUs must contain at least one SKU.");
        if (order.TotalPrice <= 0) errors.Add("TotalPrice must be greater than zero.");
        if (order.OrderTimestamp == default) errors.Add("OrderTimestamp is required.");
        return errors;
    }

    private static async Task<HttpResponseData> WriteErrorAsync(HttpRequestData request, HttpStatusCode status, string error)
    {
        var response = request.CreateResponse(status);
        await response.WriteAsJsonAsync(new { error });
        return response;
    }
}
