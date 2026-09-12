namespace CoffeeAndChill.DTOs;

public class UpdateMenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public bool IsAvailable { get; set; }
}
