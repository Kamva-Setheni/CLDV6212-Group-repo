using System.Net;
using CoffeeAndChill.Interfaces;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions;

public class DeleteMenuItemsFunction
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<DeleteMenuItemsFunction> _logger;

    public DeleteMenuItemsFunction(ITableStorageService tableStorageService, ILogger<DeleteMenuItemsFunction> logger) =>
        (_tableStorageService, _logger) = (tableStorageService, logger);

    [Function("DeleteMenuItem")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{sku}")] HttpRequestData request,
        string category,
        string sku)
    {
        try
        {
            var validationError = MenuItemValidator.ValidateRoute(category, sku);
            if (validationError is not null)
            {
                var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { errors = new[] { validationError } });
                return badRequest;
            }

            if (!await _tableStorageService.DeleteMenuItemAsync(category, sku))
            {
                var notFound = request.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteAsJsonAsync(new { error = $"No menu item found with SKU '{sku}' in category '{category}'." });
                return notFound;
            }

            _logger.LogInformation("Deleted menu item {Sku} in category {Category}.", sku, category);
            return request.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting menu item {Sku} in category {Category}.", sku, category);
            var error = request.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteAsJsonAsync(new { error = "An unexpected error occurred while deleting the menu item." });
            return error;
        }
    }
}
