using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.PdfExport;

public sealed class PdfExportSettings
{
    public PdfExportSettings(
        string fileName = "scan.pdf",
        PdfCompressionLevel compression = PdfCompressionLevel.Medium,
        string pageSize = "A4",
        bool applyEdits = true,
        bool convertToGrayscale = false)
    {
        FileName = Guard.NotWhiteSpace(fileName, nameof(fileName));
        Compression = compression;
        PageSize = Guard.NotWhiteSpace(pageSize, nameof(pageSize));
        ApplyEdits = applyEdits;
        ConvertToGrayscale = convertToGrayscale;
    }

    public string FileName { get; }

    public PdfCompressionLevel Compression { get; }

    public string PageSize { get; }

    public bool ApplyEdits { get; }

    public bool ConvertToGrayscale { get; }
}
