namespace SimpleScan.Application.Scanning;

public sealed class ScanDocumentService
{
    private readonly IScanDocumentStore _documents;
    private readonly IScannerProvider _scannerProvider;
    private readonly IScanFileStorage _fileStorage;
    private readonly TimeProvider _timeProvider;

    public ScanDocumentService(
        IScanDocumentStore documents,
        IScannerProvider scannerProvider,
        IScanFileStorage fileStorage,
        TimeProvider timeProvider)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _scannerProvider = scannerProvider ?? throw new ArgumentNullException(nameof(scannerProvider));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ScanDocument> CreateAsync(
        string scannerId,
        ScanSettings settings,
        string? name,
        CancellationToken cancellationToken)
    {
        await EnsureScannerCanScanAsync(scannerId, settings, cancellationToken);

        var document = ScanDocument.CreateNew(
            scannerId,
            settings,
            _timeProvider.GetUtcNow().UtcDateTime,
            name);

        await _documents.SaveAsync(document, cancellationToken);
        return document;
    }

    public async Task<ScanDocument> GetAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return await _documents.FindAsync(documentId, cancellationToken)
            ?? throw new DomainException($"Scan document '{documentId}' was not found.");
    }

    public async Task<ScanDocument> CloseAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await GetAsync(documentId, cancellationToken);

        document.Close(_timeProvider.GetUtcNow().UtcDateTime);
        await _documents.SaveAsync(document, cancellationToken);

        return document;
    }

    public async Task<ScanDocument> UpdateSettingsAsync(
        Guid documentId,
        ScanSettings settings,
        CancellationToken cancellationToken)
    {
        var document = await GetAsync(documentId, cancellationToken);
        await EnsureScannerCanScanAsync(document.ScannerId, settings, cancellationToken);

        document.UpdateSettings(settings);
        await _documents.SaveAsync(document, cancellationToken);

        return document;
    }

    public async Task DeleteAsync(Guid documentId, CancellationToken cancellationToken)
    {
        await _documents.DeleteAsync(documentId, cancellationToken);
        await _fileStorage.DeleteDocumentAsync(documentId, cancellationToken);
    }

    private async Task EnsureScannerCanScanAsync(
        string scannerId,
        ScanSettings settings,
        CancellationToken cancellationToken)
    {
        var status = await _scannerProvider.GetStatusAsync(scannerId, cancellationToken);
        if (!status.IsAvailable)
        {
            throw new DomainException($"Scanner '{scannerId}' is not available.");
        }

        var capabilities = await _scannerProvider.GetCapabilitiesAsync(scannerId, cancellationToken);
        ScanSettingsValidator.EnsureSupported(settings, capabilities);
    }
}
