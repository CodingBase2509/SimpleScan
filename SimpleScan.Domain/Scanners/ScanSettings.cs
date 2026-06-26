using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.Scanners;

public sealed class ScanSettings
{
    public ScanSettings(
        int dpi = 300,
        ScanColorMode colorMode = ScanColorMode.Color,
        string paperSize = "A4",
        string source = "Flatbed",
        bool duplex = false)
    {
        Dpi = Guard.Positive(dpi, nameof(dpi));
        ColorMode = colorMode;
        PaperSize = Guard.NotWhiteSpace(paperSize, nameof(paperSize));
        Source = Guard.NotWhiteSpace(source, nameof(source));
        Duplex = duplex;
    }

    public int Dpi { get; private set; }

    public ScanColorMode ColorMode { get; private set; }

    public string PaperSize { get; private set; }

    public string Source { get; private set; }

    public bool Duplex { get; private set; }

    public ScanSettings With(
        int? dpi = null,
        ScanColorMode? colorMode = null,
        string? paperSize = null,
        string? source = null,
        bool? duplex = null) =>
        new(
            dpi ?? Dpi,
            colorMode ?? ColorMode,
            paperSize ?? PaperSize,
            source ?? Source,
            duplex ?? Duplex);
}
