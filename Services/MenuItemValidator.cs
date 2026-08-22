 



namespace CoffeeNChill.Functions.Services;

public static class MenuItemValidator
{
    public static string? ValidateCreate(CreateMenuItemRequest? request)
    {
        if (request is null) return "Request body is required.";
        var routeError = ValidateRoute(request.Catergory, request.SKU);
        if (routeError is not null) return routeError;
        if (string.IsNullOrWhiteSpace(request.Name)) return "Name is required.";
        if (string.IsNullOrWhiteSpace(request.Description)) return "Description is required.";
        if (request.Price < 0) return "Price cannot be negative.";
        return null;
    }
    public static string? ValidateUpdate(UpdateMenuItemRequest? request)
    {
        if (request is null) return "Request body is required.";
        if (request.Price < 0) return "Price cannot be negative.";
        return null;
    }
    public static string? ValidateRoute(string? category, string? sku)
    {
        if (string.IsNullOrWhiteSpace(category)) return "Category is required.";
        if (string.IsNullOrWhiteSpace(sku)) return "SKU is required.";
        if (category.Contains('/') || sku.Contains('/')) return "Category and SKU cannot contain '/'.";
        return null;
    }
}