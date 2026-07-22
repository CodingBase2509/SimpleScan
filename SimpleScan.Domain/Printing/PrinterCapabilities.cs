namespace SimpleScan.Domain.Printing;

public sealed class PrinterCapabilities
{
    public PrinterCapabilities(
        IReadOnlyList<string>? supportedPaperSizes = null,
        IReadOnlyList<PrintColorMode>? supportedColorModes = null,
        bool supportsDuplex = false,
        int maxCopies = 99)
    {
        SupportedPaperSizes = supportedPaperSizes is { Count: > 0 }
            ? supportedPaperSizes
            : ["A4", "Letter"];
        SupportedColorModes = supportedColorModes is { Count: > 0 }
            ? supportedColorModes
            : [PrintColorMode.Color, PrintColorMode.Grayscale];
        SupportsDuplex = supportsDuplex;
        MaxCopies = Math.Max(1, maxCopies);
    }

    public IReadOnlyList<string> SupportedPaperSizes { get; }

    public IReadOnlyList<PrintColorMode> SupportedColorModes { get; }

    public bool SupportsDuplex { get; }

    public int MaxCopies { get; }

    public static PrinterCapabilities Default { get; } = new(supportsDuplex: true);
}
