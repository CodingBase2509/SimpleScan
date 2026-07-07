using Microsoft.AspNetCore.Components;

namespace SimpleScan.Components.Pages.Scan.Features;

public partial class ScannedPagesStrip
{
    [Parameter]
    public int PageCount { get; set; }

    [Parameter]
    public Guid? SelectedPageId { get; set; }

    private static string GetThumbnailClass(int pageNumber) =>
        pageNumber == 1
            ? "scanned-pages-strip__item scanned-pages-strip__item--selected"
            : "scanned-pages-strip__item";
}
