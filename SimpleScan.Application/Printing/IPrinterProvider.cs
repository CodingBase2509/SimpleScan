namespace SimpleScan.Application.Printing;

public interface IPrinterProvider
{
    Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken);

    Task<PrinterCapabilities> GetCapabilitiesAsync(string printerId, CancellationToken cancellationToken);

    Task<ScannerStatus> GetStatusAsync(string printerId, CancellationToken cancellationToken);

    Task<PrintJob> PrintAsync(
        string printerId,
        PrintSettings settings,
        IReadOnlyList<PrintFileInput> files,
        CancellationToken cancellationToken);
}
