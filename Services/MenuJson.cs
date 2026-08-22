using System.Text.Json;
namespace CoffeeNChill.Functions.Services;

public static class MenuJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}