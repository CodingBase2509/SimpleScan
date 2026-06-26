using SimpleScan.Domain.Scanners;

namespace SimpleScan.Application.Stores;

public interface IScannerStore
{
    Task<IReadOnlyList<ScannerDevice>> ListAsync(CancellationToken cancellationToken);

    Task SaveAsync(ScannerDevice scanner, CancellationToken cancellationToken);

    Task<ScannerDevice?> FindAsync(string scannerId, CancellationToken cancellationToken);

    Task SaveCapabilitiesAsync(
        string scannerId,
        ScannerCapabilities capabilities,
        CancellationToken cancellationToken);

    Task<ScannerCapabilities?> FindCapabilitiesAsync(
        string scannerId,
        CancellationToken cancellationToken);
}
