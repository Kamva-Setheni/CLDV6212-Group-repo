using CoffeeAndChill.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions;

public class DownloadStaffDocument
{
    private readonly IDocumentStorageService _storage;
    private readonly ILogger<DownloadStaffDocument> _logger;

    public DownloadStaffDocument(IDocumentStorageService storage, ILogger<DownloadStaffDocument> logger) =>
        (_storage, _logger) = (storage, logger);

    [Function("DownloadStaffDocument")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequest request,
        string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains("..", StringComparison.Ordinal) ||
                fileName.Contains('/') || fileName.Contains('\\'))
            {
                return new BadRequestObjectResult(new { error = "The file name is invalid." });
            }

            var stream = await _storage.DownloadDocumentAsync(fileName);
            if (stream is null)
                return new NotFoundObjectResult(new { error = $"File '{fileName}' not found." });

            _logger.LogInformation("Downloaded staff document {FileName}.", fileName);
            return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = fileName };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error downloading staff document {FileName}.", fileName);
            return new ObjectResult(new { error = "An unexpected error occurred while downloading the document." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
