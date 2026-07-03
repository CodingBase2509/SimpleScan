using System.Collections.Concurrent;
using SimpleScan.Application.Stores;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Infrastructure.Stores;

public class InMemoryScannerStore : IScannerStore
{
    private readonly ConcurrentDictionary<string, ScannerDevice> _scanner = new();
    private readonly ConcurrentDictionary<string, ScannerCapabilities>  _capabilities = new();
    
    public Task<IReadOnlyList<ScannerDevice>> ListAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        return Task.FromResult<IReadOnlyList<ScannerDevice>>(_scanner.Values.ToList());
    }

    public Task SaveAsync(ScannerDevice scanner, CancellationToken cancellationToken)
    {
        if (_scanner.TryAdd(scanner.Id, scanner))
            throw new DomainException($"Scanner with id {scanner.Id} can not be stored");
        return Task.CompletedTask;
    }

    public Task<ScannerDevice?> FindAsync(string scannerId, CancellationToken cancellationToken)
    {
        return _scanner.TryGetValue(scannerId, out var scanner) 
            ? Task.FromResult(scanner) 
            : Task.FromResult<ScannerDevice?>(null);
    }

    public Task SaveCapabilitiesAsync(string scannerId, ScannerCapabilities capabilities, CancellationToken cancellationToken)
    {
        if (_capabilities.TryAdd(scannerId, capabilities))
            throw new DomainException($"Capabilities with id {scannerId} can not be stored");
        return Task.CompletedTask;
    }

    public Task<ScannerCapabilities?> FindCapabilitiesAsync(string scannerId, CancellationToken cancellationToken)
    {
        return _capabilities.TryGetValue(scannerId, out var capabilities)
            ? Task.FromResult(capabilities)
            : Task.FromResult<ScannerCapabilities?>(null);
    }
}