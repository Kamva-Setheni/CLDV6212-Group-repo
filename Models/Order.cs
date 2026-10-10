namespace CoffeeAndChill.Models;

/// <summary>
/// The contract placed on <c>order-processing-queue</c> and consumed by the queue trigger.
/// Property names intentionally match the Part 2 brief so the HTTP producer and worker share
/// one explicit message format.
/// </summary>
public sealed class Order
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public List<string> SelectedItemSKUs { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public DateTimeOffset OrderTimestamp { get; set; }
}
