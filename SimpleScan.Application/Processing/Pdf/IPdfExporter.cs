using SimpleScan.Application.FileStorage;
using SimpleScan.Domain.Pdf;

namespace SimpleScan.Application.Processing.Pdf;

public interface IPdfExporter
{
    Task<BinaryFile> ExportAsync(
        IReadOnlyList<PdfPageInput> pages,
        PdfExportSettings settings,
        CancellationToken cancellationToken);
}
