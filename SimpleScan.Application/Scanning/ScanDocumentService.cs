namespace SimpleScan.Application.Scanning;

public sealed class ScanDocumentService
{
    private readonly IScanDocumentStore _documents;
    private readonly TimeProvider _timeProvider;

    public ScanDocumentService(IScanDocumentStore documents, TimeProvider timeProvider)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ScanDocument> CreateAsync(
        string scannerId,
        ScanSettings settings,
        string? name,
        CancellationToken cancellationToken)
    {
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

    public Task DeleteAsync(Guid documentId, CancellationToken cancellationToken) =>
        _documents.DeleteAsync(documentId, cancellationToken);
}
