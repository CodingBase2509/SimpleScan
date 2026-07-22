using SimpleScan.Application.Printing;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Printing;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Infrastructure.Printing;

public sealed class CompositePrinterProvider(
    MockPrinterProvider mockPrinterProvider,
    CupsPrinterProvider cupsPrinterProvider) : IPrinterProvider
{
    public async Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var printers = new List<ScannerDevice>();
        printers.AddRange(await mockPrinterProvider.DiscoverAsync(cancellationToken));
        printers.AddRange(await cupsPrinterProvider.DiscoverAsync(cancellationToken));

        return printers
            .GroupBy(printer => printer.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
    }

    public Task<PrinterCapabilities> GetCapabilitiesAsync(string printerId, CancellationToken cancellationToken) =>
        ResolveProvider(printerId).GetCapabilitiesAsync(printerId, cancellationToken);

    public Task<ScannerStatus> GetStatusAsync(string printerId, CancellationToken cancellationToken) =>
        ResolveProvider(printerId).GetStatusAsync(printerId, cancellationToken);

    public Task<PrintJob> PrintAsync(
        string printerId,
        PrintSettings settings,
        IReadOnlyList<PrintFileInput> files,
        CancellationToken cancellationToken) =>
        ResolveProvider(printerId).PrintAsync(printerId, settings, files, cancellationToken);

    private IPrinterProvider ResolveProvider(string printerId)
    {
        if (MockPrinterProvider.CanHandle(printerId))
        {
            return mockPrinterProvider;
        }

        if (CupsPrinterProvider.CanHandle(printerId))
        {
            return cupsPrinterProvider;
        }

        throw new DomainException($"Device '{printerId}' does not have a printer provider.");
    }
}
