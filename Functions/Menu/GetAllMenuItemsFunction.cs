using System.Net;
using CoffeeAndChill.Interfaces;
using CoffeeAndChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions;

public class GetAllMenuItemsFunction
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<GetAllMenuItemsFunction> _logger;

    public GetAllMenuItemsFunction(ITableStorageService tableStorageService, ILogger<GetAllMenuItemsFunction> logger) =>
        (_tableStorageService, _logger) = (tableStorageService, logger);

    [Function("GetAllMenuItems")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")] HttpRequestData request)
    {
        try
        {
            var menuItems = await _tableStorageService.GetAllMenuItemsAsync();
            _logger.LogInformation("Retrieved {Count} menu item(s).", menuItems.Count);
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(menuItems);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all menu items.");
            var error = request.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteAsJsonAsync(new { error = "An unexpected error occurred while retrieving menu items." });
            return error;
        }
    }
}
