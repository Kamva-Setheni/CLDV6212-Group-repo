using Azure.Data.Tables;

namespace CoffeeAndChill.Services;

/// <summary>
/// Creates and returns the shared <c>Orders</c> table client. Both HTTP status reads and
/// queue-triggered writes use this helper so they always use the same storage connection.
/// </summary>
public static class OrdersTable
{
    public const string TableName = "Orders";

    public static async Task<TableClient> GetAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
            ?? throw new InvalidOperationException("AzureWebJobsStorage is not set.");

        var client = new TableClient(connectionString, TableName);
        await client.CreateIfNotExistsAsync();
        return client;
    }
}
