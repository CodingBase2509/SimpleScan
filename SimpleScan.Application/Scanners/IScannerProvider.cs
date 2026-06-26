using SimpleScan.Application.FileStorage;
using SimpleScan.Domain.Scanning;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Application.Scanners;

public interface IScannerProvider
{
    Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken);
    Task<ScannerCapabilities> GetCapabilitiesAsync(string scannerId, CancellationToken cancellationToken);
    Task<ScannerStatus> GetStatusAsync(string scannerId, CancellationToken cancellationToken);
    Task<BinaryFile> ScanAsync(string scannerId, ScanSettings settings, CancellationToken cancellationToken);
}
