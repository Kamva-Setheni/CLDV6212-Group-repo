using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CoffeeAndChill.Interfaces;
using CoffeeAndChill.Models;

namespace CoffeeAndChill.Services;

public class DocumentStorageService : IDocumentStorageService
{
    private const string ContainerName = "staff-docs";
    private readonly BlobContainerClient _containerClient;

    public DocumentStorageService(string connectionString) =>
        _containerClient = new BlobContainerClient(connectionString, ContainerName);

    public async Task UploadDocumentAsync(string fileName, Stream fileStream, CancellationToken ct = default)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: ct);
        var blobClient = _containerClient.GetBlobClient(fileName);

        if (await blobClient.ExistsAsync(ct))
        {
            throw new RequestFailedException(409, "A document with this name already exists.");
        }

        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" }
        }, ct);
    }

    public async Task<List<StaffDocumentInfo>> ListDocumentsAsync(CancellationToken ct = default)
    {
        var results = new List<StaffDocumentInfo>();
        try
        {
            await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync(cancellationToken: ct))
            {
                var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                var properties = await blobClient.GetPropertiesAsync(cancellationToken: ct);
                results.Add(new StaffDocumentInfo
                {
                    FileName = blobItem.Name,
                    SizeInBytes = properties.Value.ContentLength,
                    LastModified = properties.Value.LastModified
                });
            }
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return results;
        }

        return results;
    }

    public async Task<Stream?> DownloadDocumentAsync(string fileName, CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync(ct)) return null;
        BlobDownloadInfo download = await blobClient.DownloadAsync(ct);
        return download.Content;
    }
}
