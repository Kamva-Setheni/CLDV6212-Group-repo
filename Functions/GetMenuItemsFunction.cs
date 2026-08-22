using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Functions
{
    public class GetMenuItemFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<GetMenuItemFunction> _logger;

        public GetMenuItemFunction(
            ITableStorageService tableStorageService,
            ILogger<GetMenuItemFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        [Function("GetMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "menuitems/{category}/{sku}")]
            HttpRequestData req,
            string category,
            string sku)
        {
            try
            {
                // --- Route parameter validation ---
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(category))
                    validationErrors.Add("Category is required.");

                if (string.IsNullOrWhiteSpace(sku))
                    validationErrors.Add("SKU is required.");

                if (validationErrors.Any())
                {
                    _logger.LogWarning("GetMenuItem validation failed: {Errors}",
                        string.Join(", ", validationErrors));

                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteAsJsonAsync(new { errors = validationErrors });
                    return bad;
                }

                _logger.LogInformation(
                    "Retrieving menu item. Category: {Category} | SKU: {SKU}",
                    category, sku);

                var menuItem = await _tableStorageService.GetMenuItemsAsync(category, sku);

                // 404 - item does not exist
                if (menuItem == null)
                {
                    _logger.LogWarning(
                        "Menu item not found. Category: {Category} | SKU: {SKU}",
                        category, sku);

                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteAsJsonAsync(new
                    {
                        error = $"No menu item found with SKU '{sku}' in category '{category}'."
                    });
                    return notFound;
                }

                _logger.LogInformation(
                    "Menu item retrieved: {Name} | Category: {Category} | SKU: {SKU}",
                    menuItem.Name, category, sku);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(menuItem);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error retrieving menu item. Category: {Category} | SKU: {SKU}",
                    category, sku);

                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new
                {
                    error = "An unexpected error occurred while retrieving the menu item."
                });
                return error;
            }
        }
    }
}