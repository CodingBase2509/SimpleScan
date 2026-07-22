using SimpleScan.Application.Printing;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Printing;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Infrastructure.Printing;

public sealed class MockPrinterProvider(TimeProvider timeProvider) : IPrinterProvider
{
    private const string PrinterId = "mock-scanner";

    private static readonly ScannerDevice Printer = new(
        PrinterId,
        "Mock Scanner",
        ScannerProtocol.Mock,
        manufacturer: "SimpleScan",
        model: "Mock multifunction device",
        networkAddress: "mock://printer",
        functions: [DeviceFunction.Print]);

    public static bool CanHandle(string printerId) =>
        string.Equals(printerId, PrinterId, StringComparison.Ordinal);

    public Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<ScannerDevice>>([Printer]);
    }

    public Task<PrinterCapabilities> GetCapabilitiesAsync(string printerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureKnownPrinter(printerId);
        return Task.FromResult(PrinterCapabilities.Default);
    }

    public Task<ScannerStatus> GetStatusAsync(string printerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureKnownPrinter(printerId);

        return Task.FromResult(new ScannerStatus(
            printerId,
            ScannerConnectionState.Online,
            timeProvider.GetUtcNow().UtcDateTime,
            "Mock printer is ready."));
    }

    public Task<PrintJob> PrintAsync(
        string printerId,
        PrintSettings settings,
        IReadOnlyList<PrintFileInput> files,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureKnownPrinter(printerId);
        ArgumentNullException.ThrowIfNull(settings);

        if (files.Count == 0)
        {
            throw new DomainException("Cannot submit an empty print job.");
        }

        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var message = $"Mock printed {files.Count} file{(files.Count == 1 ? string.Empty : "s")} with {settings.Copies} cop{(settings.Copies == 1 ? "y" : "ies")}.";

        return Task.FromResult(new PrintJob(
            $"mock-print-{Guid.NewGuid():N}",
            printerId,
            PrintJobStatus.Completed,
            createdAtUtc,
            message));
    }

    private static void EnsureKnownPrinter(string printerId)
    {
        if (!CanHandle(printerId))
        {
            throw new DomainException($"Printer '{printerId}' is not known by the mock printer provider.");
        }
    }
}
