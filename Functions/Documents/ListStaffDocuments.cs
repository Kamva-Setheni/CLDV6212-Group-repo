using CoffeeAndChill.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions;

public class ListStaffDocuments
{
    private readonly IDocumentStorageService _storage;
    private readonly ILogger<ListStaffDocuments> _logger;

    public ListStaffDocuments(IDocumentStorageService storage, ILogger<ListStaffDocuments> logger) =>
        (_storage, _logger) = (storage, logger);

    [Function("ListStaffDocuments")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest request)
    {
        try
        {
            var files = await _storage.ListDocumentsAsync();
            _logger.LogInformation("Retrieved {Count} staff document(s).", files.Count);
            return new OkObjectResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error listing staff documents.");
            return new ObjectResult(new { error = "An unexpected error occurred while listing documents." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
