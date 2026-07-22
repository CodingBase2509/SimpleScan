using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Processing.Images;
using SimpleScan.Application.Stores;

namespace SimpleScan.Application.Printing;

public sealed class PrintDocumentService
{
    private static readonly string[] PreviewableImageExtensions =
    [
        ".jpg", ".jpeg", ".png", ".svg", ".tif", ".tiff", ".bmp", ".gif", ".webp"
    ];

    private readonly IScanDocumentStore _documents;
    private readonly IScanFileStorage _fileStorage;
    private readonly IImageProcessor _imageProcessor;
    private readonly TimeProvider _timeProvider;

    public PrintDocumentService(
        IScanDocumentStore documents,
        IScanFileStorage fileStorage,
        IImageProcessor imageProcessor,
        TimeProvider timeProvider)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ScanDocument> CreateAsync(
        string printerDeviceId,
        string? name,
        CancellationToken cancellationToken)
    {
        var document = ScanDocument.CreateNew(
            printerDeviceId,
            new ScanSettings(),
            _timeProvider.GetUtcNow().UtcDateTime,
            name);

        await _documents.SaveAsync(document, cancellationToken);
        return document;
    }

    public async Task<ScanDocument> AddUploadedPageAsync(
        Guid documentId,
        BinaryFile file,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        var document = await _documents.FindAsync(documentId, cancellationToken)
            ?? throw new DomainException($"Print document '{documentId}' was not found.");

        var pageNumber = document.Pages.Count + 1;
        StoredFile? storedOriginal = null;
        StoredFile? storedPreview = null;
        StoredFile? storedThumbnail = null;

        try
        {
            storedOriginal = await _fileStorage.SaveOriginalPageAsync(
                document.Id,
                pageNumber,
                file,
                cancellationToken);

            var page = document.AddPage(storedOriginal.Path, _timeProvider.GetUtcNow().UtcDateTime);

            if (CanCreateImagePreview(storedOriginal.Path))
            {
                storedPreview = await CreatePreviewAsync(document.Id, page.PageNumber, storedOriginal.Path, cancellationToken);
                storedThumbnail = await CreateThumbnailAsync(document.Id, page.PageNumber, storedOriginal.Path, cancellationToken);

                page.SetPreviewPath(storedPreview.Path);
                page.SetThumbnailPath(storedThumbnail.Path);
            }

            await _documents.SaveAsync(document, cancellationToken);
            return document;
        }
        catch
        {
            await DeleteFileIfPresentAsync(storedOriginal?.Path, CancellationToken.None);
            await DeleteFileIfPresentAsync(storedPreview?.Path, CancellationToken.None);
            await DeleteFileIfPresentAsync(storedThumbnail?.Path, CancellationToken.None);
            throw;
        }
    }

    private static bool CanCreateImagePreview(string path) =>
        PreviewableImageExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    private async Task<StoredFile> CreatePreviewAsync(
        Guid documentId,
        int pageNumber,
        string originalPath,
        CancellationToken cancellationToken)
    {
        await using var source = await _fileStorage.OpenReadAsync(originalPath, cancellationToken);
        var preview = await _imageProcessor.CreatePreviewAsync(
            source,
            new ImageProcessingOptions(PageEditSettings.None),
            cancellationToken);

        return await _fileStorage.SavePreviewAsync(documentId, pageNumber, preview, cancellationToken);
    }

    private async Task<StoredFile> CreateThumbnailAsync(
        Guid documentId,
        int pageNumber,
        string originalPath,
        CancellationToken cancellationToken)
    {
        await using var source = await _fileStorage.OpenReadAsync(originalPath, cancellationToken);
        var thumbnail = await _imageProcessor.CreateThumbnailAsync(
            source,
            new ImageProcessingOptions(PageEditSettings.None),
            cancellationToken);

        return await _fileStorage.SaveThumbnailAsync(documentId, pageNumber, thumbnail, cancellationToken);
    }

    private async Task DeleteFileIfPresentAsync(string? path, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(path) && await _fileStorage.ExistsAsync(path, cancellationToken))
        {
            await _fileStorage.DeleteFileAsync(path, cancellationToken);
        }
    }
}
