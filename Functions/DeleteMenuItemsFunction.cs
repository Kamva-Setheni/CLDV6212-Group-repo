using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions.Functions
{
    public class DeleteMenuItemFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<DeleteMenuItemFunction> _logger;

        public DeleteMenuItemFunction(
            ITableStorageService tableStorageService,
            ILogger<DeleteMenuItemFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "delete",
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
                    _logger.LogWarning("DeleteMenuItem validation failed: {Errors}",
                        string.Join(", ", validationErrors));

                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteAsJsonAsync(new { errors = validationErrors });
                    return bad;
                }

                _logger.LogInformation(
                    "Deleting menu item. Category: {Category} | SKU: {SKU}",
                    category, sku);

                bool deleted = await _tableStorageService.DeleteMenuItemAsync(category, sku);

                // 404 - item does not exist in storage
                if (!deleted)
                {
                    _logger.LogWarning(
                        "Menu item not found for deletion. Category: {Category} | SKU: {SKU}",
                        category, sku);

                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteAsJsonAsync(new
                    {
                        error = $"No menu item found with SKU '{sku}' in category '{category}'."
                    });
                    return notFound;
                }

                // 204 - successfully deleted, no body needed
                _logger.LogInformation(
                    "Menu item deleted. Category: {Category} | SKU: {SKU}",
                    category, sku);

                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error deleting menu item. Category: {Category} | SKU: {SKU}",
                    category, sku);

                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new
                {
                    error = "An unexpected error occurred while deleting the menu item."
                });
                return error;
            }
        }
    }
}