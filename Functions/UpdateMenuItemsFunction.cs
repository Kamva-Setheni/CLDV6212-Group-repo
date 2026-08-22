using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Functions.Functions
{
    public class UpdateMenuItemFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<UpdateMenuItemFunction> _logger;

        public UpdateMenuItemFunction(
            ITableStorageService tableStorageService,
            ILogger<UpdateMenuItemFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "put",
                Route = "menuitems/{category}/{sku}")]
            HttpRequestData req,
            string category,
            string sku)
        {
            try
            { 

                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(category))
                    validationErrors.Add("Category is required.");

                if (string.IsNullOrWhiteSpace(sku))
                    validationErrors.Add("SKU is required.");

                if (validationErrors.Any())
                {
                    _logger.LogWarning("UpdateMenuItem route validation failed: {Errors}",
                        string.Join(", ", validationErrors));

                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteAsJsonAsync(new { errors = validationErrors });
                    return bad;
                }

               
                var request = await JsonSerializer.DeserializeAsync<UpdateMenuItemRequest>(
                    req.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

               
                if (request == null)
                {
                    _logger.LogWarning("UpdateMenuItem received a null or malformed request body.");

                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteAsJsonAsync(new { error = "Request body is missing or malformed." });
                    return bad;
                }

             
                if (request.Price <= 0)
                    validationErrors.Add("Price must be greater than zero.");

                if (string.IsNullOrWhiteSpace(request.Name))
                    validationErrors.Add("Name is required.");

                if (validationErrors.Any())
                {
                    _logger.LogWarning("UpdateMenuItem body validation failed: {Errors}",
                        string.Join(", ", validationErrors));

                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteAsJsonAsync(new { errors = validationErrors });
                    return bad;
                }

                _logger.LogInformation(
                    "Updating menu item. Category: {Category} | SKU: {SKU} | Name: {Name} | Price: {Price:C}",
                    category, sku, request.Name, request.Price);

                var updatedMenuItem = await _tableStorageService.UpdateMenuItemAsync(
                    category, sku, request);

                 
                if (updatedMenuItem == null)
                {
                    _logger.LogWarning(
                        "Menu item not found for update. Category: {Category} | SKU: {SKU}",
                        category, sku);

                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteAsJsonAsync(new
                    {
                        error = $"No menu item found with SKU '{sku}' in category '{category}'."
                    });
                    return notFound;
                }

                _logger.LogInformation(
                    "Menu item updated successfully. Category: {Category} | SKU: {SKU}",
                    category, sku);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(updatedMenuItem);
                return response;
            }
            catch (InvalidOperationException ex)
            {
               
                _logger.LogWarning(ex,
                    "Conflict updating menu item. Category: {Category} | SKU: {SKU}",
                    category, sku);

                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteAsJsonAsync(new { error = ex.Message });
                return conflict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error updating menu item. Category: {Category} | SKU: {SKU}",
                    category, sku);

                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new
                {
                    error = "An unexpected error occurred while updating the menu item."
                });
                return error;
            }
        }
    }
}