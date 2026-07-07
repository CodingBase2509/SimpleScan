using Microsoft.AspNetCore.Components;
using SimpleScan.Application.Scanners;
using SimpleScan.Application.Scanning;
using SimpleScan.Domain.Documents;
using SimpleScan.Domain.Scanners;
using SimpleScan.Domain.Scanning;

namespace SimpleScan.ViewModels;

public sealed class ScanPageViewModel(
    ScanDocumentService scanDocumentService,
    ScanPageService scanPageService,
    ScannerService scannerService,
    NavigationManager navigationManager)
{
    public event Action? StateChanged;

    public Guid DocumentId { get; private set; }

    public Guid? SelectedPageId { get; private set; }

    public bool IsLoading { get; private set; }

    public bool IsScanning { get; private set; }

    public bool IsUpdatingPages { get; private set; }

    public string? ErrorMessage { get; private set; }

    public ScanDocument? Document { get; private set; }

    public ScannerDevice? Scanner { get; private set; }

    public ScannerCapabilities? Capabilities { get; private set; }

    public ScanSettings? Settings => Document?.Settings;

    public IReadOnlyList<ScannedPage> Pages => Document?.Pages ?? [];

    public ScannedPage? SelectedPage =>
        SelectedPageId is null
            ? null
            : Pages.FirstOrDefault(page => page.Id == SelectedPageId.Value);

    public string ScannerName => Scanner?.Name ?? Document?.ScannerId ?? "Scanner";

    public bool CanScan =>
        Document is not null &&
        Document.Status is not ScanDocumentStatus.Closed &&
        !IsLoading &&
        !IsScanning &&
        !IsUpdatingPages;

    public bool CanEditPage => SelectedPage is not null && !IsLoading && !IsScanning && !IsUpdatingPages;

    public bool CanSave => Document is not null && Pages.Count > 0 && !IsLoading && !IsScanning && !IsUpdatingPages;

    public string? PreviewUrl =>
        SelectedPage?.PreviewPath is null
            ? null
            : GetPreviewUrl(DocumentId, SelectedPage.Id);

    public async Task InitializeAsync(Guid documentId, Guid? pageId)
    {
        DocumentId = documentId;
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            Document = await scanDocumentService.GetAsync(documentId, CancellationToken.None);
            Scanner = await scannerService.FindAsync(Document.ScannerId, CancellationToken.None);
            Capabilities = await scannerService.GetCapabilitiesAsync(
                Document.ScannerId,
                refresh: false,
                CancellationToken.None);

            SelectedPageId = ResolveSelectedPageId(pageId, Document.Pages);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            Document = null;
            Scanner = null;
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
        navigationManager.NavigateTo($"/scan/{DocumentId}/{pageId}");
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public async Task ScanPageAsync()
    {
        if (!CanScan)
        {
            return;
        }

        IsScanning = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var page = await scanPageService.ScanAsync(DocumentId, CancellationToken.None);
            Document = await scanDocumentService.GetAsync(DocumentId, CancellationToken.None);
            SelectedPageId = page.Id;
            navigationManager.NavigateTo($"/scan/{DocumentId}/{page.Id}");
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsScanning = false;
            NotifyStateChanged();
        }
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

    public static string GetPreviewUrl(Guid documentId, Guid pageId) =>
        $"/scan-files/documents/{documentId}/pages/{pageId}/preview";

    public static string GetThumbnailUrl(Guid documentId, Guid pageId) =>
        $"/scan-files/documents/{documentId}/pages/{pageId}/thumbnail";

    private static Guid? ResolveSelectedPageId(Guid? requestedPageId, IReadOnlyList<ScannedPage> pages)
    {
        if (requestedPageId is not null && pages.Any(page => page.Id == requestedPageId.Value))
        {
            return requestedPageId;
        }

        return pages.FirstOrDefault()?.Id;
    }

    private async Task MovePageAsync(Guid pageId, int newPageNumber)
    {
        IsUpdatingPages = true;
        ErrorMessage = null;
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
            ? $"/scan/{DocumentId}"
            : $"/scan/{DocumentId}/{SelectedPageId}";

        navigationManager.NavigateTo(target);
    }

    private void NotifyStateChanged() =>
        StateChanged?.Invoke();
}
