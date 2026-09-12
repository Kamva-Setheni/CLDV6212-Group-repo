using System.Net;
using System.Text.Json;
using CoffeeAndChill.DTOs;
using CoffeeAndChill.Interfaces;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions;

public class UpdateMenuItemsFunction
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<UpdateMenuItemsFunction> _logger;

    public UpdateMenuItemsFunction(ITableStorageService tableStorageService, ILogger<UpdateMenuItemsFunction> logger) =>
        (_tableStorageService, _logger) = (tableStorageService, logger);

    [Function("UpdateMenuItem")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{sku}")] HttpRequestData request,
        string category,
        string sku)
    {
        try
        {
            var routeError = MenuItemValidator.ValidateRoute(category, sku);
            if (routeError is not null)
            {
                var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { errors = new[] { routeError } });
                return badRequest;
            }

            var body = await JsonSerializer.DeserializeAsync<UpdateMenuItemRequest>(request.Body, MenuJson.Options);
            var validationError = MenuItemValidator.ValidateUpdate(body);
            if (validationError is not null)
            {
                var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { errors = new[] { validationError } });
                return badRequest;
            }

            var menuItem = await _tableStorageService.UpdateMenuItemAsync(category, sku, body!);
            if (menuItem is null)
            {
                var notFound = request.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteAsJsonAsync(new { error = $"No menu item found with SKU '{sku}' in category '{category}'." });
                return notFound;
            }

            _logger.LogInformation("Updated menu item {Sku} in category {Category}.", sku, category);
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(menuItem);
            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Malformed UpdateMenuItem request body.");
            var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { errors = new[] { "Request body is malformed." } });
            return badRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating menu item {Sku} in category {Category}.", sku, category);
            var error = request.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteAsJsonAsync(new { error = "An unexpected error occurred while updating the menu item." });
            return error;
        }
    }
}
