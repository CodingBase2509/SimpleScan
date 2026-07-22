using Microsoft.AspNetCore.Components;
using SimpleScan.Domain.Documents;
using SimpleScan.ViewModels;

namespace SimpleScan.Components.Pages.Scan.Features;

public partial class ScannedPagesStrip
{
    [Parameter]
    public Guid DocumentId { get; set; }

    [Parameter]
    public IReadOnlyList<ScannedPage> Pages { get; set; } = [];

    [Parameter]
    public string AriaLabel { get; set; } = "Scanned pages";

    [Parameter]
    public string EmptyMessage { get; set; } = "No pages scanned yet";

    [Parameter]
    public Guid? SelectedPageId { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback<Guid> PageSelected { get; set; }

    [Parameter]
    public EventCallback<Guid> MovePageEarlier { get; set; }

    [Parameter]
    public EventCallback<Guid> MovePageLater { get; set; }

    [Parameter]
    public EventCallback<Guid> DeletePage { get; set; }

    private string GetItemClass(ScannedPage page) =>
        page.Id == SelectedPageId
            ? "scanned-pages-strip__item scanned-pages-strip__item--selected"
            : "scanned-pages-strip__item";

    private static string GetOpenPageLabel(ScannedPage page) =>
        $"Open page {page.PageNumber}";

    private static string GetThumbnailAlt(ScannedPage page) =>
        $"Page {page.PageNumber} thumbnail";

    private static bool CanShowThumbnail(ScannedPage page) =>
        page.ThumbnailPath is not null && IsImagePath(page.ThumbnailPath);

    private static bool IsImagePath(string path) =>
        Path.GetExtension(path).ToLowerInvariant() is ".jpg" or ".jpeg" or ".png" or ".svg" or ".tif" or ".tiff" or ".bmp" or ".gif" or ".webp";

    private static string GetFileExtensionLabel(ScannedPage page)
    {
        var extension = Path.GetExtension(page.OriginalPath).TrimStart('.').ToUpperInvariant();
        return string.IsNullOrWhiteSpace(extension) ? "FILE" : extension;
    }

    private static string GetMoveEarlierLabel(ScannedPage page) =>
        $"Move page {page.PageNumber} left";

    private static string GetMoveLaterLabel(ScannedPage page) =>
        $"Move page {page.PageNumber} right";

    private static string GetDeleteLabel(ScannedPage page) =>
        $"Delete page {page.PageNumber}";
}
