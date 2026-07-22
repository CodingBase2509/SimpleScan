using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.Printing;

public sealed class PrintSettings
{
    public PrintSettings(
        int copies = 1,
        string paperSize = "A4",
        PrintColorMode colorMode = PrintColorMode.Color,
        PrintDuplexMode duplexMode = PrintDuplexMode.OneSided)
    {
        Copies = Guard.Positive(copies, nameof(copies));
        PaperSize = Guard.NotWhiteSpace(paperSize, nameof(paperSize));
        ColorMode = colorMode;
        DuplexMode = duplexMode;
    }

    public int Copies { get; }

    public string PaperSize { get; }

    public PrintColorMode ColorMode { get; }

    public PrintDuplexMode DuplexMode { get; }
}
