using SimpleScan.Domain.Documents;

namespace SimpleScan.Application.Stores;

public interface IScanDocumentStore
{
    Task<ScanDocument?> FindAsync(Guid documentId, CancellationToken cancellationToken);

    Task SaveAsync(ScanDocument document, CancellationToken cancellationToken);

    Task DeleteAsync(Guid documentId, CancellationToken cancellationToken);
}
