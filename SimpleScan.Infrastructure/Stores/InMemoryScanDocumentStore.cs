using System.Collections.Concurrent;
using SimpleScan.Application.Stores;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Documents;

namespace SimpleScan.Infrastructure.Stores;

public class InMemoryScanDocumentStore : IScanDocumentStore
{
    private readonly ConcurrentDictionary<Guid, ScanDocument> _documents = new();
    
    public Task<ScanDocument?> FindAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return _documents.TryGetValue(documentId, out var doc) 
            ? Task.FromResult(doc) 
            : Task.FromResult<ScanDocument?>(null);
    }

    public Task SaveAsync(ScanDocument document, CancellationToken cancellationToken)
    {
        if (_documents.TryAdd(document.Id, document))
            throw new DomainException($"Document with id {document.Id} can not be stored");
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid documentId, CancellationToken cancellationToken)
    {
        if (documentId == Guid.Empty || documentId == default)
            return Task.CompletedTask;
        
        _documents.TryRemove(documentId, out _);
        return Task.CompletedTask;
    }
}