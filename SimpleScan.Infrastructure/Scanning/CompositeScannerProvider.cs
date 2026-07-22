using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Scanners;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Scanners;
using SimpleScan.Domain.Scanning;

namespace SimpleScan.Infrastructure.Scanning;

public sealed class CompositeScannerProvider(
    MockScannerProvider mockScannerProvider,
    Naps2ScannerProvider naps2ScannerProvider) : IScannerProvider
{
    public async Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var scanners = new List<ScannerDevice>();
        scanners.AddRange(await mockScannerProvider.DiscoverAsync(cancellationToken));
        scanners.AddRange(await naps2ScannerProvider.DiscoverAsync(cancellationToken));

        return scanners
            .GroupBy(scanner => scanner.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
    }

    public Task<ScannerCapabilities> GetCapabilitiesAsync(string scannerId, CancellationToken cancellationToken) =>
        ResolveProvider(scannerId).GetCapabilitiesAsync(scannerId, cancellationToken);

    public Task<ScannerStatus> GetStatusAsync(string scannerId, CancellationToken cancellationToken) =>
        ResolveProvider(scannerId).GetStatusAsync(scannerId, cancellationToken);

    public Task<BinaryFile> ScanAsync(string scannerId, ScanSettings settings, CancellationToken cancellationToken) =>
        ResolveProvider(scannerId).ScanAsync(scannerId, settings, cancellationToken);

    private IScannerProvider ResolveProvider(string scannerId)
    {
        if (MockScannerProvider.CanHandle(scannerId))
        {
            return mockScannerProvider;
        }

        if (Naps2ScannerProvider.CanHandle(scannerId))
        {
            return naps2ScannerProvider;
        }

        throw new DomainException($"Device '{scannerId}' does not have a scanner provider.");
    }
}
