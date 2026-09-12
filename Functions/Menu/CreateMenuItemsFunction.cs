using System.Net;
using System.Text.Json;
using CoffeeAndChill.DTOs;
using CoffeeAndChill.Interfaces;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions;

public class CreateMenuItemsFunction
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<CreateMenuItemsFunction> _logger;

    public CreateMenuItemsFunction(ITableStorageService tableStorageService, ILogger<CreateMenuItemsFunction> logger) =>
        (_tableStorageService, _logger) = (tableStorageService, logger);

    [Function("CreateMenuItem")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequestData request)
    {
        try
        {
            var body = await JsonSerializer.DeserializeAsync<CreateMenuItemRequest>(request.Body, MenuJson.Options);
            var validationError = MenuItemValidator.ValidateCreate(body);
            if (validationError is not null)
            {
                var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { errors = new[] { validationError } });
                return badRequest;
            }

            var menuItem = await _tableStorageService.CreateMenuItemAsync(body!);
            _logger.LogInformation("Created menu item {Sku} in category {Category}.", body!.SKU, body.Category);
            var response = request.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(menuItem);
            return response;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Menu item creation conflict.");
            var conflict = request.CreateResponse(HttpStatusCode.Conflict);
            await conflict.WriteAsJsonAsync(new { error = ex.Message });
            return conflict;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Malformed CreateMenuItem request body.");
            var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { errors = new[] { "Request body is malformed." } });
            return badRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating menu item.");
            var error = request.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteAsJsonAsync(new { error = "An unexpected error occurred. Please try again later." });
            return error;
        }
    }
}
