using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace CoffeeAndChill.Services;

/// <summary>
/// Provides the Base64-encoded order queue client required by the producer and the Functions
/// queue extension. Keeping the encoding explicit prevents incompatible producer/consumer data.
/// </summary>
public static class OrderQueue
{
    public const string QueueName = "order-processing-queue";

    public static async Task<QueueClient> GetAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
            ?? throw new InvalidOperationException("AzureWebJobsStorage is not set.");

        var options = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };
        var client = new QueueClient(connectionString, QueueName, options);
        await client.CreateIfNotExistsAsync();
        return client;
    }
}
