using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Stores;

namespace SimpleScan.Application.Printing;

public sealed class PrintJobService
{
    private readonly IScanDocumentStore _documents;
    private readonly IPrinterProvider _printerProvider;

    public PrintJobService(
        IScanDocumentStore documents,
        IPrinterProvider printerProvider)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _printerProvider = printerProvider ?? throw new ArgumentNullException(nameof(printerProvider));
    }

    public async Task<PrintJob> PrintAsync(
        Guid documentId,
        PrintSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var document = await _documents.FindAsync(documentId, cancellationToken)
            ?? throw new DomainException($"Print document '{documentId}' was not found.");

        if (document.Pages.Count == 0)
        {
            throw new DomainException("Cannot print without uploaded files.");
        }

        var status = await _printerProvider.GetStatusAsync(document.ScannerId, cancellationToken);
        if (!status.IsAvailable)
        {
            throw new DomainException($"Printer '{document.ScannerId}' is not available.");
        }

        var capabilities = await _printerProvider.GetCapabilitiesAsync(document.ScannerId, cancellationToken);
        EnsureSupported(settings, capabilities);

        var files = document.Pages
            .OrderBy(page => page.PageNumber)
            .Select(page => new PrintFileInput(
                page.Id,
                page.PageNumber,
                page.OriginalPath,
                Path.GetFileName(page.OriginalPath),
                GetContentType(page.OriginalPath)))
            .ToArray();

        return await _printerProvider.PrintAsync(
            document.ScannerId,
            settings,
            files,
            cancellationToken);
    }

    private static void EnsureSupported(PrintSettings settings, PrinterCapabilities capabilities)
    {
        if (settings.Copies > capabilities.MaxCopies)
        {
            throw new DomainException($"Printer supports at most {capabilities.MaxCopies} copies.");
        }

        if (!capabilities.SupportedPaperSizes.Contains(settings.PaperSize, StringComparer.OrdinalIgnoreCase))
        {
            throw new DomainException($"Paper size '{settings.PaperSize}' is not supported by the printer.");
        }

        if (!capabilities.SupportedColorModes.Contains(settings.ColorMode))
        {
            throw new DomainException($"Color mode '{settings.ColorMode}' is not supported by the printer.");
        }

        if (settings.DuplexMode != PrintDuplexMode.OneSided && !capabilities.SupportsDuplex)
        {
            throw new DomainException("Duplex printing is not supported by the printer.");
        }
    }

    private static string GetContentType(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".tif" or ".tiff" => "image/tiff",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".rtf" => "application/rtf",
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".odt" => "application/vnd.oasis.opendocument.text",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".odp" => "application/vnd.oasis.opendocument.presentation",
            _ => "application/octet-stream"
        };
}
