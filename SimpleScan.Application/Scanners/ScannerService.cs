namespace SimpleScan.Application.Scanners;

public sealed class ScannerService
{
    private readonly IScannerProvider _scannerProvider;
    private readonly IScannerStore _scanners;
    private readonly IEventPublisher _events;
    private readonly TimeProvider _timeProvider;

    public ScannerService(
        IScannerProvider scannerProvider,
        IScannerStore scanners,
        IEventPublisher events,
        TimeProvider timeProvider)
    {
        _scannerProvider = scannerProvider ?? throw new ArgumentNullException(nameof(scannerProvider));
        _scanners = scanners ?? throw new ArgumentNullException(nameof(scanners));
        _events = events ?? throw new ArgumentNullException(nameof(events));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var scanners = await _scannerProvider.DiscoverAsync(cancellationToken);

        foreach (var scanner in scanners)
        {
            await _scanners.SaveAsync(scanner, cancellationToken);

            await _events.PublishAsync(
                NewEvent(EventTypes.Scanner.Discovered, scanner.Id, scanner),
                cancellationToken);
        }

        return scanners;
    }

    public async Task<ScannerCapabilities> GetCapabilitiesAsync(
        string scannerId,
        bool refresh,
        CancellationToken cancellationToken)
    {
        if (!refresh)
        {
            var cachedCapabilities = await _scanners.FindCapabilitiesAsync(scannerId, cancellationToken);
            if (cachedCapabilities is not null)
            {
                return cachedCapabilities;
            }
        }

        var capabilities = await _scannerProvider.GetCapabilitiesAsync(scannerId, cancellationToken);
        await _scanners.SaveCapabilitiesAsync(scannerId, capabilities, cancellationToken);

        return capabilities;
    }

    public Task<ScannerStatus> GetStatusAsync(string scannerId, CancellationToken cancellationToken) =>
        _scannerProvider.GetStatusAsync(scannerId, cancellationToken);

    public Task<ScannerDevice?> FindAsync(string scannerId, CancellationToken cancellationToken) =>
        _scanners.FindAsync(scannerId, cancellationToken);

    public Task<IReadOnlyList<ScannerDevice>> ListKnownAsync(CancellationToken cancellationToken) =>
        _scanners.ListAsync(cancellationToken);

    private AppEvent NewEvent(string type, string? scannerId = null, object? payload = null) =>
        new(type, _timeProvider.GetUtcNow().UtcDateTime, scannerId, payload: payload);
}
