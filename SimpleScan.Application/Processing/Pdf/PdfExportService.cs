using SimpleScan.Application.Downloads;
using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Processing.Images;
using SimpleScan.Application.Stores;
using SimpleScan.Domain.Downloads;
using SimpleScan.Domain.Pdf;

namespace SimpleScan.Application.Processing.Pdf;

public sealed class PdfExportService
{
    private static readonly TimeSpan DownloadTicketLifetime = TimeSpan.FromHours(1);

    private readonly IScanDocumentStore _documents;
    private readonly IScanFileStorage _fileStorage;
    private readonly IImageProcessor _imageProcessor;
    private readonly IPdfExporter _pdfExporter;
    private readonly DownloadTicketService _downloadTickets;

    public PdfExportService(
        IScanDocumentStore documents,
        IScanFileStorage fileStorage,
        IImageProcessor imageProcessor,
        IPdfExporter pdfExporter,
        DownloadTicketService downloadTickets)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
        _pdfExporter = pdfExporter ?? throw new ArgumentNullException(nameof(pdfExporter));
        _downloadTickets = downloadTickets ?? throw new ArgumentNullException(nameof(downloadTickets));
    }

    public async Task<DownloadTicket> ExportAsync(
        Guid documentId,
        PdfExportSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var document = await _documents.FindAsync(documentId, cancellationToken)
            ?? throw new DomainException($"Scan document '{documentId}' was not found.");

        document.MarkExporting();
        await _documents.SaveAsync(document, cancellationToken);

        var openedStreams = new List<Stream>();

        try
        {
            var pages = new List<PdfPageInput>();

            foreach (var page in document.Pages.OrderBy(page => page.PageNumber))
            {
                var source = await _fileStorage.OpenReadAsync(page.OriginalPath, cancellationToken);
                openedStreams.Add(source);

                var renderedPage = await _imageProcessor.RenderForPdfAsync(
                    source,
                    new ImageProcessingOptions(page.EditSettings),
                    cancellationToken);

                openedStreams.Add(renderedPage.Content);

                pages.Add(new PdfPageInput(
                    page.Id,
                    page.PageNumber,
                    renderedPage.Content,
                    renderedPage.ContentType));
            }

            var pdf = await _pdfExporter.ExportAsync(pages, settings, cancellationToken);
            await using (pdf.Content)
            {
                var storedPdf = await _fileStorage.SavePdfExportAsync(
                    document.Id,
                    pdf,
                    cancellationToken);

                document.MarkExported();
                await _documents.SaveAsync(document, cancellationToken);

                return await _downloadTickets.CreatePdfExportTicketAsync(
                    document.Id,
                    storedPdf.Path,
                    storedPdf.FileName,
                    DownloadTicketLifetime,
                    cancellationToken);
            }
        }
        catch
        {
            document.MarkFailed();
            await _documents.SaveAsync(document, CancellationToken.None);
            throw;
        }
        finally
        {
            foreach (var stream in openedStreams)
            {
                await stream.DisposeAsync();
            }
        }
    }
}
