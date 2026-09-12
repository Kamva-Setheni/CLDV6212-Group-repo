using Azure;
using CoffeeAndChill.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeAndChill.Functions;

public class UploadStaffDocument
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/png", "image/jpeg"
    };

    private readonly IDocumentStorageService _storage;
    private readonly ILogger<UploadStaffDocument> _logger;

    public UploadStaffDocument(IDocumentStorageService storage, ILogger<UploadStaffDocument> logger) =>
        (_storage, _logger) = (storage, logger);

    [Function("UploadStaffDocument")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequest request)
    {
        try
        {
            if (!request.HasFormContentType || request.Form.Files.Count == 0)
                return new BadRequestObjectResult(new { error = "No file uploaded." });

            var file = request.Form.Files[0];
            if (file.Length == 0)
                return new BadRequestObjectResult(new { error = "Uploaded file is empty." });

            var fileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(fileName) || file.FileName.Contains("..", StringComparison.Ordinal) ||
                file.FileName.Contains('/') || file.FileName.Contains('\\'))
            {
                return new BadRequestObjectResult(new { error = "The file name is invalid." });
            }

            var extension = Path.GetExtension(fileName);
            if (!AllowedExtensions.Contains(extension) || !AllowedMimeTypes.Contains(file.ContentType))
            {
                return new BadRequestObjectResult(new
                {
                    error = "Only PDF, Word, PNG, and JPEG staff documents are allowed."
                });
            }

            await using var stream = file.OpenReadStream();
            await _storage.UploadDocumentAsync(fileName, stream);
            _logger.LogInformation("Uploaded staff document {FileName}.", fileName);
            return new OkObjectResult(new { message = "Uploaded", fileName });
        }
        catch (RequestFailedException ex) when (ex.Status == StatusCodes.Status409Conflict)
        {
            _logger.LogWarning("A staff document already exists: {Message}", ex.Message);
            return new ConflictObjectResult(new { error = "A document with this name already exists." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading a staff document.");
            return new ObjectResult(new { error = "An unexpected error occurred while uploading the document." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
