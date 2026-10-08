using System.Text.Json;
using Azure.Data.Tables;
using CoffeeAndChill.Models;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions.Orders;

/// <summary>
/// Converts a queued order into an Orders-table record and visibly advances it through the
/// barista lifecycle. Invalid messages are deliberately rethrown so the host retries them and
/// finally moves them to <c>order-processing-queue-poison</c>.
/// </summary>
public sealed class ProcessOrderQueue
{
    private static readonly string[] Lifecycle = ["Preparing", "Ready", "Collected"];
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly ILogger<ProcessOrderQueue> _logger;

    public ProcessOrderQueue(ILogger<ProcessOrderQueue> logger) => _logger = logger;

    [Function("ProcessOrderQueue")]
    public async Task Run(
        [QueueTrigger(OrderQueue.QueueName, Connection = "AzureWebJobsStorage")] string message,
        FunctionContext context)
    {
        var attempt = context.BindingContext.BindingData.TryGetValue("DequeueCount", out var value)
            ? value?.ToString() ?? "unknown"
            : "unknown";
        _logger.LogInformation("Order message received on attempt {Attempt}.", attempt);

        Order? order;
        try
        {
            order = JsonSerializer.Deserialize<Order>(message, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Malformed order JSON on attempt {Attempt}. Payload: {Payload}", attempt, message);
            throw;
        }

        if (order is null || string.IsNullOrWhiteSpace(order.OrderId) || order.OrderTimestamp == default)
        {
            _logger.LogError("Order payload is incomplete on attempt {Attempt}. Payload: {Payload}", attempt, message);
            throw new InvalidOperationException("Order payload requires a valid OrderId and OrderTimestamp.");
        }

        var table = await OrdersTable.GetAsync();
        var partitionKey = order.OrderTimestamp.ToUniversalTime().ToString("yyyy-MM-dd");
        var received = new TableEntity(partitionKey, order.OrderId)
        {
            ["CustomerName"] = order.CustomerName ?? string.Empty,
            ["SelectedItemSKUs"] = string.Join(',', order.SelectedItemSKUs ?? []),
            ["TotalPrice"] = (double)order.TotalPrice,
            ["OrderTimestamp"] = order.OrderTimestamp,
            ["Status"] = "Received",
            ["LastUpdated"] = DateTimeOffset.UtcNow
        };
        await table.UpsertEntityAsync(received, TableUpdateMode.Replace);
        _logger.LogInformation("Order {OrderId} was stored as Received in {PartitionKey}.", order.OrderId, partitionKey);

        foreach (var status in Lifecycle)
        {
            await Task.Delay(TimeSpan.FromSeconds(GetStepDelaySeconds()));
            var update = new TableEntity(partitionKey, order.OrderId)
            {
                ["Status"] = status,
                ["LastUpdated"] = DateTimeOffset.UtcNow
            };
            await table.UpsertEntityAsync(update, TableUpdateMode.Merge);
            _logger.LogInformation("Order {OrderId} moved to {Status}.", order.OrderId, status);
        }
    }

    private static int GetStepDelaySeconds() =>
        int.TryParse(Environment.GetEnvironmentVariable("ORDER_STEP_DELAY_SECONDS"), out var seconds) && seconds is >= 0 and <= 60
            ? seconds
            : 5;
}
