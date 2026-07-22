namespace SimpleScan.Application.Printing;

public sealed class PrinterService
{
    private readonly IPrinterProvider _printerProvider;
    private readonly IScannerStore _devices;
    private readonly IEventPublisher _events;
    private readonly TimeProvider _timeProvider;

    public PrinterService(
        IPrinterProvider printerProvider,
        IScannerStore devices,
        IEventPublisher events,
        TimeProvider timeProvider)
    {
        _printerProvider = printerProvider ?? throw new ArgumentNullException(nameof(printerProvider));
        _devices = devices ?? throw new ArgumentNullException(nameof(devices));
        _events = events ?? throw new ArgumentNullException(nameof(events));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var printers = await _printerProvider.DiscoverAsync(cancellationToken);

        foreach (var printer in printers)
        {
            await _devices.SaveAsync(printer, cancellationToken);

            await _events.PublishAsync(
                NewEvent("printer.discovered", printer.Id, printer),
                cancellationToken);
        }

        return printers;
    }

    public Task<PrinterCapabilities> GetCapabilitiesAsync(string printerId, CancellationToken cancellationToken) =>
        _printerProvider.GetCapabilitiesAsync(printerId, cancellationToken);

    public Task<ScannerStatus> GetStatusAsync(string printerId, CancellationToken cancellationToken) =>
        _printerProvider.GetStatusAsync(printerId, cancellationToken);

    private AppEvent NewEvent(string type, string? printerId = null, object? payload = null) =>
        new(type, _timeProvider.GetUtcNow().UtcDateTime, printerId, payload: payload);
}
