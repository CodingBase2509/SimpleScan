using System.Collections.Concurrent;
using SimpleScan.Application.Stores;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Infrastructure.Stores;

public sealed class InMemoryScannerStore : IScannerStore
{
    private readonly ConcurrentDictionary<string, ScannerDevice> _scanners = new();
    private readonly ConcurrentDictionary<string, ScannerCapabilities>  _capabilities = new();
    
    public Task<IReadOnlyList<ScannerDevice>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<IReadOnlyList<ScannerDevice>>(_scanners.Values.ToList());
    }

    public Task SaveAsync(ScannerDevice scanner, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scanner);
        cancellationToken.ThrowIfCancellationRequested();

        _scanners[scanner.Id] = scanner;
        return Task.CompletedTask;
    }

    public Task<ScannerDevice?> FindAsync(string scannerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(scannerId))
        {
            return Task.FromResult<ScannerDevice?>(null);
        }

        return _scanners.TryGetValue(scannerId, out var scanner) 
            ? Task.FromResult(scanner) 
            : Task.FromResult<ScannerDevice?>(null);
    }

    public Task SaveCapabilitiesAsync(string scannerId, ScannerCapabilities capabilities, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(scannerId))
        {
            throw new ArgumentException("Scanner id cannot be empty.", nameof(scannerId));
        }

        ArgumentNullException.ThrowIfNull(capabilities);
        cancellationToken.ThrowIfCancellationRequested();

        _capabilities[scannerId.Trim()] = capabilities;
        return Task.CompletedTask;
    }

    public Task<ScannerCapabilities?> FindCapabilitiesAsync(string scannerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(scannerId))
        {
            return Task.FromResult<ScannerCapabilities?>(null);
        }

        return _capabilities.TryGetValue(scannerId, out var capabilities)
            ? Task.FromResult(capabilities)
            : Task.FromResult<ScannerCapabilities?>(null);
    }
}
