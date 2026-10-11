namespace CoffeeAndChill.DTOs;

/// <summary>
/// Request body for POST /api/orders/queue. Nullable members let the producer report a
/// clear validation error instead of failing during deserialisation.
/// </summary>
public sealed class CreateOrderRequest
{
    public string? OrderId { get; set; }
    public string? CustomerName { get; set; }
    public List<string>? SelectedItemSKUs { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTimeOffset? OrderTimestamp { get; set; }
}
