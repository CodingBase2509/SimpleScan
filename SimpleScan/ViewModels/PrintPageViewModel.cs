using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Printing;
using SimpleScan.Application.Scanners;
using SimpleScan.Application.Scanning;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Documents;
using SimpleScan.Domain.Printing;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.ViewModels;

public sealed class PrintPageViewModel(
    ScanDocumentService scanDocumentService,
    ScanPageService scanPageService,
    PrintDocumentService printDocumentService,
    PrintJobService printJobService,
    PrinterService printerService,
    ScannerService scannerService,
    NavigationManager navigationManager)
{
    private const long MaxUploadSize = 50L * 1024L * 1024L;
    private static readonly string[] SupportedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".svg", ".tif", ".tiff", ".bmp", ".gif", ".webp",
        ".pdf",
        ".doc", ".docx", ".rtf", ".odt", ".txt", ".html", ".htm",
        ".xls", ".xlsx", ".csv", ".ods",
        ".ppt", ".pptx", ".odp"
    ];

    public event Action? StateChanged;

    public Guid DocumentId { get; private set; }

    public Guid? SelectedPageId { get; private set; }

    public bool IsLoading { get; private set; }

    public bool IsUploading { get; private set; }

    public bool IsUpdatingPages { get; private set; }

    public bool IsCancelling { get; private set; }

    public bool IsPrinting { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string? StatusMessage { get; private set; }

    public ScanDocument? Document { get; private set; }

    public ScannerDevice? Printer { get; private set; }

    public PrinterCapabilities? Capabilities { get; private set; }

    public PrintSettings Settings { get; private set; } = new();

    public PrintJob? LastPrintJob { get; private set; }

    public IReadOnlyList<ScannedPage> Pages => Document?.Pages ?? [];

    public ScannedPage? SelectedPage =>
        SelectedPageId is null
            ? null
            : Pages.FirstOrDefault(page => page.Id == SelectedPageId.Value);

    public string PrinterName => Printer?.Name ?? Document?.ScannerId ?? "Printer";

    private bool HasBusyAction =>
        IsLoading || IsUploading || IsUpdatingPages || IsCancelling || IsPrinting;

    public bool CanUpload => Document is not null && !HasBusyAction;

    public bool CanCancel => Document is not null && !HasBusyAction;

    public bool CanPrint => Document is not null && Printer?.CanPrint == true && Pages.Count > 0 && !HasBusyAction;

    public string? PreviewUrl =>
        SelectedPage is null
            ? null
            : SelectedPagePreviewKind == PagePreviewKind.Pdf
                ? GetOriginalUrl(DocumentId, SelectedPage.Id)
                : SelectedPage.PreviewPath is not null
                    ? ScanPageViewModel.GetPreviewUrl(DocumentId, SelectedPage.Id)
                    : null;

    public PagePreviewKind SelectedPagePreviewKind =>
        SelectedPage is null
            ? PagePreviewKind.Image
            : GetPreviewKind(SelectedPage.OriginalPath);

    public string? SelectedPageFileName =>
        SelectedPage?.OriginalPath is null
            ? null
            : Path.GetFileName(SelectedPage.OriginalPath);

    public async Task InitializeAsync(Guid documentId, Guid? pageId)
    {
        DocumentId = documentId;
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            Document = await scanDocumentService.GetAsync(documentId, CancellationToken.None);
            Printer = await scannerService.FindAsync(Document.ScannerId, CancellationToken.None);
            Capabilities = await printerService.GetCapabilitiesAsync(Document.ScannerId, CancellationToken.None);
            SelectedPageId = ResolveSelectedPageId(pageId, Document.Pages);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            Document = null;
            Printer = null;
            Capabilities = null;
            SelectedPageId = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public Task SelectPageAsync(Guid pageId)
    {
        if (Pages.All(page => page.Id != pageId))
        {
            return Task.CompletedTask;
        }

        SelectedPageId = pageId;
        navigationManager.NavigateTo($"/print/{DocumentId}/{pageId}");
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public async Task UploadPagesAsync(IReadOnlyList<IBrowserFile> files)
    {
        if (!CanUpload || files.Count == 0)
        {
            return;
        }

        IsUploading = true;
        ErrorMessage = null;
            StatusMessage = $"Uploading {files.Count} file{(files.Count == 1 ? string.Empty : "s")}.";
        NotifyStateChanged();

        try
        {
            foreach (var file in files)
            {
                ValidateUpload(file);

                await using var stream = file.OpenReadStream(MaxUploadSize);
                var binaryFile = new BinaryFile(
                    stream,
                    file.Name,
                    GetContentType(file),
                    file.Size);

                Document = await printDocumentService.AddUploadedPageAsync(
                    DocumentId,
                    binaryFile,
                    CancellationToken.None);

                SelectedPageId = Document.Pages.LastOrDefault()?.Id;
            }

            StatusMessage = $"{files.Count} file{(files.Count == 1 ? string.Empty : "s")} uploaded.";
            NavigateToSelectedPage();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusMessage = null;
        }
        finally
        {
            IsUploading = false;
            NotifyStateChanged();
        }
    }

    public Task UpdateSettingsAsync(PrintSettings settings)
    {
        if (HasBusyAction)
        {
            return Task.CompletedTask;
        }

        Settings = settings;
        ErrorMessage = null;
        StatusMessage = null;
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public async Task MovePageEarlierAsync(Guid pageId)
    {
        var page = Pages.FirstOrDefault(candidate => candidate.Id == pageId);
        if (page is null || page.PageNumber <= 1)
        {
            return;
        }

        await MovePageAsync(pageId, page.PageNumber - 1);
    }

    public async Task MovePageLaterAsync(Guid pageId)
    {
        var page = Pages.FirstOrDefault(candidate => candidate.Id == pageId);
        if (page is null || page.PageNumber >= Pages.Count)
        {
            return;
        }

        await MovePageAsync(pageId, page.PageNumber + 1);
    }

    public async Task DeletePageAsync(Guid pageId)
    {
        if (Pages.All(page => page.Id != pageId))
        {
            return;
        }

        IsUpdatingPages = true;
        ErrorMessage = null;
        StatusMessage = null;
        NotifyStateChanged();

        try
        {
            Document = await scanPageService.DeletePageAsync(DocumentId, pageId, CancellationToken.None);
            SelectedPageId = ResolveSelectedPageIdAfterDelete(pageId, Document.Pages);
            NavigateToSelectedPage();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsUpdatingPages = false;
            NotifyStateChanged();
        }
    }

    public async Task CancelAsync()
    {
        if (!CanCancel)
        {
            return;
        }

        IsCancelling = true;
        ErrorMessage = null;
        StatusMessage = null;
        NotifyStateChanged();

        try
        {
            await scanDocumentService.DeleteAsync(DocumentId, CancellationToken.None);
            Document = null;
            Printer = null;
            SelectedPageId = null;
            navigationManager.NavigateTo("/");
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsCancelling = false;
            NotifyStateChanged();
        }
    }

    public async Task PrintAsync()
    {
        if (!CanPrint)
        {
            return;
        }

        IsPrinting = true;
        StatusMessage = "Submitting print job.";
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            LastPrintJob = await printJobService.PrintAsync(
                DocumentId,
                Settings,
                CancellationToken.None);

            StatusMessage = LastPrintJob.Message ?? $"Print job {LastPrintJob.Status}.";
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusMessage = null;
        }
        finally
        {
            IsPrinting = false;
            NotifyStateChanged();
        }
    }

    private async Task MovePageAsync(Guid pageId, int newPageNumber)
    {
        IsUpdatingPages = true;
        ErrorMessage = null;
        StatusMessage = null;
        NotifyStateChanged();

        try
        {
            Document = await scanPageService.MovePageAsync(
                DocumentId,
                pageId,
                newPageNumber,
                CancellationToken.None);

            SelectedPageId = pageId;
            NavigateToSelectedPage();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsUpdatingPages = false;
            NotifyStateChanged();
        }
    }

    private static void ValidateUpload(IBrowserFile file)
    {
        if (file.Size <= 0)
        {
            throw new DomainException($"'{file.Name}' is empty.");
        }

        if (file.Size > MaxUploadSize)
        {
            throw new DomainException($"'{file.Name}' is larger than the 50 MB upload limit.");
        }

        var extension = Path.GetExtension(file.Name);
        if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new DomainException($"'{file.Name}' is not a supported printable file.");
        }
    }

    private static string GetContentType(IBrowserFile file)
    {
        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            return file.ContentType;
        }

        return Path.GetExtension(file.Name).ToLowerInvariant() switch
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

    private static PagePreviewKind GetPreviewKind(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".pdf" => PagePreviewKind.Pdf,
            ".jpg" or ".jpeg" or ".png" or ".svg" or ".tif" or ".tiff" or ".bmp" or ".gif" or ".webp" => PagePreviewKind.Image,
            _ => PagePreviewKind.File
        };

    private static string GetOriginalUrl(Guid documentId, Guid pageId) =>
        $"/scan-files/documents/{documentId}/pages/{pageId}/original";

    private static Guid? ResolveSelectedPageId(Guid? requestedPageId, IReadOnlyList<ScannedPage> pages)
    {
        if (requestedPageId is not null && pages.Any(page => page.Id == requestedPageId.Value))
        {
            return requestedPageId;
        }

        return pages.FirstOrDefault()?.Id;
    }

    private Guid? ResolveSelectedPageIdAfterDelete(Guid deletedPageId, IReadOnlyList<ScannedPage> pages)
    {
        if (SelectedPageId != deletedPageId && pages.Any(page => page.Id == SelectedPageId))
        {
            return SelectedPageId;
        }

        return pages.FirstOrDefault()?.Id;
    }

    private void NavigateToSelectedPage()
    {
        var target = SelectedPageId is null
            ? $"/print/{DocumentId}"
            : $"/print/{DocumentId}/{SelectedPageId}";

        navigationManager.NavigateTo(target);
    }

    private void NotifyStateChanged() =>
        StateChanged?.Invoke();
}
