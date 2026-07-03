using System.Collections.Concurrent;
using SimpleScan.Application.Stores;
using SimpleScan.Domain.Documents;

namespace SimpleScan.Infrastructure.Stores;

public sealed class InMemoryScanDocumentStore : IScanDocumentStore
{
    private readonly ConcurrentDictionary<Guid, ScanDocument> _documents = new();
    
    public Task<ScanDocument?> FindAsync(Guid documentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _documents.TryGetValue(documentId, out var doc) 
            ? Task.FromResult(doc) 
            : Task.FromResult<ScanDocument?>(null);
    }

    public Task SaveAsync(ScanDocument document, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid documentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (documentId == Guid.Empty)
        {
            return Task.CompletedTask;
        }
        
        _documents.TryRemove(documentId, out _);
        return Task.CompletedTask;
    }
}
