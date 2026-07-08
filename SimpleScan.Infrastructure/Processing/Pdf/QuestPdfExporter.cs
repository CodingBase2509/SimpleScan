using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Processing.Pdf;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Pdf;

namespace SimpleScan.Infrastructure.Processing.Pdf;

public sealed class QuestPdfExporter : IPdfExporter
{
    public QuestPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<BinaryFile> ExportAsync(
        IReadOnlyList<PdfPageInput> pages,
        PdfExportSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (pages.Count == 0)
        {
            throw new DomainException("Cannot export a PDF without pages.");
        }

        var pageContents = new List<PdfPageContent>();

        foreach (var page in pages.OrderBy(page => page.PageNumber))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (page.ImageContent.CanSeek)
            {
                page.ImageContent.Position = 0;
            }

            using var buffer = new MemoryStream();
            await page.ImageContent.CopyToAsync(buffer, cancellationToken);

            pageContents.Add(new PdfPageContent(
                page.PageNumber,
                buffer.ToArray(),
                page.ContentType));
        }

        var pdf = Document
            .Create(container =>
            {
                foreach (var page in pageContents)
                {
                    container.Page(pdfPage =>
                    {
                        pdfPage.Size(GetPageSize(settings.PageSize));
                        pdfPage.Margin(0);
                        pdfPage.Content().Element(content => ComposePage(content, page));
                    });
                }
            })
            .GeneratePdf();

        var output = new MemoryStream(pdf);
        return new BinaryFile(output, settings.FileName, "application/pdf", output.Length);
    }

    private static PageSize GetPageSize(string pageSize)
    {
        return pageSize.Equals("Letter", StringComparison.OrdinalIgnoreCase)
            ? PageSizes.Letter
            : PageSizes.A4;
    }

    private static void ComposePage(IContainer container, PdfPageContent page)
    {
        if (page.ContentType.Equals("image/svg+xml", StringComparison.OrdinalIgnoreCase))
        {
            var svg = System.Text.Encoding.UTF8.GetString(page.Content);
            container.AlignCenter().AlignMiddle().Svg(svg);
            return;
        }

        container.Image(page.Content).FitArea();
    }

    private sealed record PdfPageContent(
        int PageNumber,
        byte[] Content,
        string ContentType);
}
