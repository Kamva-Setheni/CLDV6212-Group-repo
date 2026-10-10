using System.Net;
using Azure.Data.Tables;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions.Orders;

/// <summary>
/// Returns the latest lifecycle status for an order. The lookup searches by RowKey because the
/// client knows the order ID but does not need to know the date-based table partition.
/// </summary>
public sealed class GetOrderStatus
{
    private readonly ILogger<GetOrderStatus> _logger;

    public GetOrderStatus(ILogger<GetOrderStatus> logger) => _logger = logger;

    [Function("GetOrderStatus")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{orderId}/status")] HttpRequestData request,
        string orderId)
    {
        try
        {
            var table = await OrdersTable.GetAsync();
            TableEntity? found = null;
            await foreach (var row in table.QueryAsync<TableEntity>(entity => entity.RowKey == orderId))
            {
                found = row;
                break;
            }

            if (found is null)
            {
                _logger.LogWarning("Status requested for unknown order {OrderId}.", orderId);
                var missing = request.CreateResponse(HttpStatusCode.NotFound);
                await missing.WriteAsJsonAsync(new { error = $"Order '{orderId}' was not found." });
                return missing;
            }

            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                orderId = found.RowKey,
                orderDate = found.PartitionKey,
                customerName = found.GetString("CustomerName"),
                totalPrice = found.GetDouble("TotalPrice"),
                status = found.GetString("Status"),
                lastUpdated = found.GetDateTimeOffset("LastUpdated")
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving status for {OrderId}.", orderId);
            var failure = request.CreateResponse(HttpStatusCode.InternalServerError);
            await failure.WriteAsJsonAsync(new { error = "An unexpected error occurred while retrieving the order status." });
            return failure;
        }
    }
}
