using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Scanners;
using SimpleScan.Domain.Scanners;
using SimpleScan.Domain.Scanning;
using System.Text;

namespace SimpleScan.Infrastructure.Scanning;

public sealed class MockScannerProvider : IScannerProvider
{
    private const string ScannerId = "mock-scanner";
    private static readonly ScannerDevice Scanner = new(
        ScannerId,
        "Mock Scanner",
        ScannerProtocol.Mock,
        manufacturer: "SimpleScan",
        model: "Mock",
        networkAddress: "mock://scanner",
        functions: [DeviceFunction.Scan, DeviceFunction.Print]);

    public static bool CanHandle(string scannerId) =>
        string.Equals(scannerId, ScannerId, StringComparison.Ordinal);

    public Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<ScannerDevice>>([Scanner]);
    }

    public Task<ScannerCapabilities> GetCapabilitiesAsync(string scannerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureKnownScanner(scannerId);
        return Task.FromResult(ScannerCapabilities.Default);
    }

    public Task<ScannerStatus> GetStatusAsync(string scannerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureKnownScanner(scannerId);

        return Task.FromResult(new ScannerStatus(
            scannerId,
            ScannerConnectionState.Online,
            DateTime.UtcNow,
            "Mock scanner is ready."));
    }

    public Task<BinaryFile> ScanAsync(string scannerId, ScanSettings settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureKnownScanner(scannerId);
        ArgumentNullException.ThrowIfNull(settings);

        var svg = CreateMockScanSvg(settings);
        var content = new MemoryStream(Encoding.UTF8.GetBytes(svg));

        return Task.FromResult(new BinaryFile(
            content,
            $"mock-scan-{DateTime.UtcNow:yyyyMMdd-HHmmss}.svg",
            "image/svg+xml",
            content.Length));
    }

    private static void EnsureKnownScanner(string scannerId)
    {
        if (!CanHandle(scannerId))
        {
            throw new InvalidOperationException($"Scanner '{scannerId}' is not known by the mock scanner provider.");
        }
    }

    private static string CreateMockScanSvg(ScanSettings settings)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

        return $$"""
                 <svg xmlns="http://www.w3.org/2000/svg" width="1240" height="1754" viewBox="0 0 1240 1754">
                   <rect width="1240" height="1754" fill="#ffffff"/>
                   <rect x="72" y="72" width="1096" height="1610" fill="none" stroke="#222222" stroke-width="4"/>
                   <text x="120" y="170" font-family="Arial, sans-serif" font-size="56" fill="#111111">SimpleScan Mock Scan</text>
                   <text x="120" y="260" font-family="Arial, sans-serif" font-size="34" fill="#444444">DPI: {{settings.Dpi}}</text>
                   <text x="120" y="318" font-family="Arial, sans-serif" font-size="34" fill="#444444">Color mode: {{settings.ColorMode}}</text>
                   <text x="120" y="376" font-family="Arial, sans-serif" font-size="34" fill="#444444">Paper: {{settings.PaperSize}}</text>
                   <text x="120" y="434" font-family="Arial, sans-serif" font-size="34" fill="#444444">Source: {{settings.Source}}</text>
                   <text x="120" y="492" font-family="Arial, sans-serif" font-size="34" fill="#444444">Duplex: {{settings.Duplex}}</text>
                   <text x="120" y="580" font-family="Arial, sans-serif" font-size="28" fill="#666666">Generated: {{timestamp}}</text>
                   <line x1="120" y1="700" x2="1120" y2="700" stroke="#cccccc" stroke-width="3"/>
                   <line x1="120" y1="780" x2="1120" y2="780" stroke="#dddddd" stroke-width="3"/>
                   <line x1="120" y1="860" x2="1120" y2="860" stroke="#dddddd" stroke-width="3"/>
                   <line x1="120" y1="940" x2="1120" y2="940" stroke="#dddddd" stroke-width="3"/>
                   <rect x="120" y="1050" width="420" height="420" fill="#f2f6fb" stroke="#7a9cc6" stroke-width="3"/>
                   <circle cx="880" cy="1260" r="210" fill="#f8f1e7" stroke="#c28f5c" stroke-width="3"/>
                 </svg>
                 """;
    }
}
