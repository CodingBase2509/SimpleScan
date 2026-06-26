namespace SimpleScan.Application.FileStorage;

public interface IScanFileStorage
{
    Task<StoredFile> SaveOriginalPageAsync(
        Guid documentId,
        int pageNumber,
        BinaryFile file,
        CancellationToken cancellationToken);

    Task<StoredFile> SavePreviewAsync(
        Guid documentId,
        int pageNumber,
        BinaryFile file,
        CancellationToken cancellationToken);

    Task<StoredFile> SaveThumbnailAsync(
        Guid documentId,
        int pageNumber,
        BinaryFile file,
        CancellationToken cancellationToken);

    Task<StoredFile> SavePdfExportAsync(
        Guid documentId,
        BinaryFile file,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken);

    Task DeleteFileAsync(string path, CancellationToken cancellationToken);

    Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken);
}
