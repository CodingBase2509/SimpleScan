namespace SimpleScan.Application.Scanning;

public sealed class ScanPageService
{
    private readonly IScanDocumentStore _documents;
    private readonly IScannerProvider _scannerProvider;
    private readonly IScanFileStorage _fileStorage;
    private readonly IImageProcessor _imageProcessor;
    private readonly IEventPublisher _events;
    private readonly TimeProvider _timeProvider;

    public ScanPageService(
        IScanDocumentStore documents,
        IScannerProvider scannerProvider,
        IScanFileStorage fileStorage,
        IImageProcessor imageProcessor,
        IEventPublisher events,
        TimeProvider timeProvider)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _scannerProvider = scannerProvider ?? throw new ArgumentNullException(nameof(scannerProvider));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
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
            await AddPreviewAndThumbnailAsync(document, page, storedFile.Path, cancellationToken);

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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            document.MarkReady();
            await _documents.SaveAsync(document, CancellationToken.None);

            await _events.PublishAsync(
                NewEvent(EventTypes.Scan.Cancelled, document.ScannerId, document.Id),
                CancellationToken.None);

            throw;
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

    public async Task<ScannedPage> UpdatePageEditSettingsAsync(
        Guid documentId,
        Guid pageId,
        PageEditSettings editSettings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(editSettings);

        var document = await GetDocumentAsync(documentId, cancellationToken);
        var page = document.GetPage(pageId);

        page.UpdateEditSettings(editSettings);
        await RecreatePreviewAndThumbnailAsync(document, page, cancellationToken);
        await _documents.SaveAsync(document, cancellationToken);

        return page;
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

    private async Task AddPreviewAndThumbnailAsync(
        ScanDocument document,
        ScannedPage page,
        string originalPath,
        CancellationToken cancellationToken)
    {
        StoredFile? storedPreview = null;
        StoredFile? storedThumbnail = null;

        try
        {
            storedPreview = await CreatePreviewAsync(document, page, originalPath, cancellationToken);
            storedThumbnail = await CreateThumbnailAsync(document, page, originalPath, cancellationToken);
        }
        catch
        {
            await DeleteFileIfPresentAsync(storedPreview?.Path, CancellationToken.None);
            await DeleteFileIfPresentAsync(storedThumbnail?.Path, CancellationToken.None);
            throw;
        }

        page.SetPreviewPath(storedPreview.Path);
        page.SetThumbnailPath(storedThumbnail.Path);

        await _events.PublishAsync(
            NewEvent(EventTypes.Page.PreviewReady, document.ScannerId, document.Id, page.Id),
            cancellationToken);

        await _events.PublishAsync(
            NewEvent(EventTypes.Page.ThumbnailReady, document.ScannerId, document.Id, page.Id),
            cancellationToken);
    }

    private async Task<StoredFile> CreatePreviewAsync(
        ScanDocument document,
        ScannedPage page,
        string originalPath,
        CancellationToken cancellationToken)
    {
        await using var previewSource = await _fileStorage.OpenReadAsync(originalPath, cancellationToken);
        var preview = await _imageProcessor.CreatePreviewAsync(
            previewSource,
            new ImageProcessingOptions(page.EditSettings),
            cancellationToken);

        return await _fileStorage.SavePreviewAsync(
            document.Id,
            page.PageNumber,
            preview,
            cancellationToken);
    }

    private async Task<StoredFile> CreateThumbnailAsync(
        ScanDocument document,
        ScannedPage page,
        string originalPath,
        CancellationToken cancellationToken)
    {
        await using var thumbnailSource = await _fileStorage.OpenReadAsync(originalPath, cancellationToken);
        var thumbnail = await _imageProcessor.CreateThumbnailAsync(
            thumbnailSource,
            new ImageProcessingOptions(page.EditSettings),
            cancellationToken);

        return await _fileStorage.SaveThumbnailAsync(
            document.Id,
            page.PageNumber,
            thumbnail,
            cancellationToken);
    }

    private async Task RecreatePreviewAndThumbnailAsync(
        ScanDocument document,
        ScannedPage page,
        CancellationToken cancellationToken)
    {
        var previousPreviewPath = page.PreviewPath;
        var previousThumbnailPath = page.ThumbnailPath;

        await AddPreviewAndThumbnailAsync(document, page, page.OriginalPath, cancellationToken);

        await DeletePreviousFileIfChangedAsync(previousPreviewPath, page.PreviewPath, cancellationToken);
        await DeletePreviousFileIfChangedAsync(previousThumbnailPath, page.ThumbnailPath, cancellationToken);
    }

    private async Task DeletePreviousFileIfChangedAsync(
        string? previousPath,
        string? currentPath,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(previousPath, currentPath, StringComparison.Ordinal))
        {
            await DeleteFileIfPresentAsync(previousPath, cancellationToken);
        }
    }

    private AppEvent NewEvent(string type, string? scannerId = null, Guid? documentId = null, Guid? pageId = null) =>
        new(type, _timeProvider.GetUtcNow().UtcDateTime, scannerId, documentId, pageId);
}
