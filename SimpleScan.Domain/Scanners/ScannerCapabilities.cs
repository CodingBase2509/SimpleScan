namespace SimpleScan.Domain.Scanners;

public sealed class ScannerCapabilities
{
    public ScannerCapabilities(
        IReadOnlyCollection<int> supportedDpi,
        IReadOnlyCollection<ScanColorMode> supportedColorModes,
        IReadOnlyCollection<string> supportedPaperSizes,
        IReadOnlyCollection<string> supportedSources,
        bool supportsDuplex)
    {
        SupportedDpi = supportedDpi.Where(dpi => dpi > 0).Distinct().Order().ToArray();
        SupportedColorModes = supportedColorModes.Distinct().ToArray();
        SupportedPaperSizes = NormalizeValues(supportedPaperSizes);
        SupportedSources = NormalizeValues(supportedSources);
        SupportsDuplex = supportsDuplex;
    }

    public IReadOnlyList<int> SupportedDpi { get; }

    public IReadOnlyList<ScanColorMode> SupportedColorModes { get; }

    public IReadOnlyList<string> SupportedPaperSizes { get; }

    public IReadOnlyList<string> SupportedSources { get; }

    public bool SupportsDuplex { get; }

    public static ScannerCapabilities Default { get; } = new(
        [150, 200, 300],
        [ScanColorMode.Color, ScanColorMode.Grayscale, ScanColorMode.BlackAndWhite],
        ["A4"],
        ["Flatbed"],
        supportsDuplex: false);

    private static IReadOnlyList<string> NormalizeValues(IReadOnlyCollection<string> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
