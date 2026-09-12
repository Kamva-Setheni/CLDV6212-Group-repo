using System.Net;
using CoffeeAndChill.Interfaces;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions;

public class GetMenuItemsByCategoryFunction
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<GetMenuItemsByCategoryFunction> _logger;

    public GetMenuItemsByCategoryFunction(ITableStorageService tableStorageService, ILogger<GetMenuItemsByCategoryFunction> logger) =>
        (_tableStorageService, _logger) = (tableStorageService, logger);

    [Function("GetMenuItemsByCategory")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequestData request,
        string category)
    {
        try
        {
            var validationError = MenuItemValidator.ValidateCategory(category);
            if (validationError is not null)
            {
                var badRequest = request.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { errors = new[] { validationError } });
                return badRequest;
            }

            var menuItems = await _tableStorageService.GetMenuItemsByCategoryAsync(category);
            _logger.LogInformation("Retrieved {Count} item(s) for menu category {Category}.", menuItems.Count, category);
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(menuItems);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving menu category {Category}.", category);
            var error = request.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteAsJsonAsync(new { error = "An unexpected error occurred while retrieving menu items." });
            return error;
        }
    }
}
