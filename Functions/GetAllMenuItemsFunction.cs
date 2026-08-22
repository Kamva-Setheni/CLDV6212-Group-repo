using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Functions
{
    public class GetAllMenuItemsFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<GetAllMenuItemsFunction> _logger;

        public GetAllMenuItemsFunction(
            ITableStorageService tableStorageService,
            ILogger<GetAllMenuItemsFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "menuitems")]
            HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Retrieving all menu items.");

                var menuItems = await _tableStorageService.GetAllMenuItemsAsync();

                // Return empty array rather than 404 — valid state, just no items yet
                if (menuItems == null || !menuItems.Any())
                {
                    _logger.LogWarning("No menu items found in storage.");

                    var empty = req.CreateResponse(HttpStatusCode.OK);
                    await empty.WriteAsJsonAsync(new
                    {
                        message = "No menu items are currently available.",
                        items = Array.Empty<object>()
                    });
                    return empty;
                }

                _logger.LogInformation(
                    "Retrieved {Count} menu item(s).", menuItems.Count());

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(menuItems);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving all menu items.");

                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new
                {
                    error = "An unexpected error occurred while retrieving menu items."
                });
                return error;
            }
        }
    }
}