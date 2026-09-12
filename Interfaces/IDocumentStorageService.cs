using CoffeeAndChill.Models;

namespace CoffeeAndChill.Interfaces;

public interface IDocumentStorageService
{
    Task UploadDocumentAsync(string fileName, Stream fileStream, CancellationToken ct = default);
    Task<List<StaffDocumentInfo>> ListDocumentsAsync(CancellationToken ct = default);
    Task<Stream?> DownloadDocumentAsync(string fileName, CancellationToken ct = default);
}
