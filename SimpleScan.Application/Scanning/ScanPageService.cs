namespace SimpleScan.Application.Scanning;

public sealed class ScanPageService
{
    private readonly IScanDocumentStore _documents;
    private readonly IScannerProvider _scannerProvider;
    private readonly IScanFileStorage _fileStorage;
    private readonly IEventPublisher _events;
    private readonly TimeProvider _timeProvider;

    public ScanPageService(
        IScanDocumentStore documents,
        IScannerProvider scannerProvider,
        IScanFileStorage fileStorage,
        IEventPublisher events,
        TimeProvider timeProvider)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _scannerProvider = scannerProvider ?? throw new ArgumentNullException(nameof(scannerProvider));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _events = events ?? throw new ArgumentNullException(nameof(events));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ScannedPage> ScanAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await GetDocumentAsync(documentId, cancellationToken);

        try
        {
            document.MarkScanning();
            await _documents.SaveAsync(document, cancellationToken);

            await _events.PublishAsync(
                NewEvent(EventTypes.Scan.Started, document.ScannerId, document.Id),
                cancellationToken);

            var scannedFile = await _scannerProvider.ScanAsync(
                document.ScannerId,
                document.Settings,
                cancellationToken);

            var pageNumber = document.Pages.Count + 1;
            var storedFile = await _fileStorage.SaveOriginalPageAsync(
                document.Id,
                pageNumber,
                scannedFile,
                cancellationToken);

            var page = document.AddPage(storedFile.Path, _timeProvider.GetUtcNow().UtcDateTime);
            document.MarkReady();
            await _documents.SaveAsync(document, cancellationToken);

            await _events.PublishAsync(
                NewEvent(EventTypes.Page.Scanned, document.ScannerId, document.Id, page.Id),
                cancellationToken);

            await _events.PublishAsync(
                NewEvent(EventTypes.Scan.Completed, document.ScannerId, document.Id, page.Id),
                cancellationToken);

            return page;
        }
        catch
        {
            document.MarkFailed();
            await _documents.SaveAsync(document, cancellationToken);

            await _events.PublishAsync(
                NewEvent(EventTypes.Scan.Failed, document.ScannerId, document.Id),
                cancellationToken);

            throw;
        }
    }

    public async Task<ScanDocument> DeletePageAsync(
        Guid documentId,
        Guid pageId,
        CancellationToken cancellationToken)
    {
        var document = await GetDocumentAsync(documentId, cancellationToken);
        var page = document.GetPage(pageId);

        await DeleteFileIfPresentAsync(page.OriginalPath, cancellationToken);
        await DeleteFileIfPresentAsync(page.PreviewPath, cancellationToken);
        await DeleteFileIfPresentAsync(page.ThumbnailPath, cancellationToken);

        document.RemovePage(pageId);
        await _documents.SaveAsync(document, cancellationToken);

        return document;
    }

    public async Task<ScanDocument> MovePageAsync(
        Guid documentId,
        Guid pageId,
        int newPageNumber,
        CancellationToken cancellationToken)
    {
        var document = await GetDocumentAsync(documentId, cancellationToken);
        document.MovePage(pageId, newPageNumber);
        await _documents.SaveAsync(document, cancellationToken);
        return document;
    }

    private async Task<ScanDocument> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return await _documents.FindAsync(documentId, cancellationToken)
            ?? throw new DomainException($"Scan document '{documentId}' was not found.");
    }

    private async Task DeleteFileIfPresentAsync(string? path, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(path) && await _fileStorage.ExistsAsync(path, cancellationToken))
        {
            await _fileStorage.DeleteFileAsync(path, cancellationToken);
        }
    }

    private AppEvent NewEvent(string type, string? scannerId = null, Guid? documentId = null, Guid? pageId = null) =>
        new(type, _timeProvider.GetUtcNow().UtcDateTime, scannerId, documentId, pageId);
}
