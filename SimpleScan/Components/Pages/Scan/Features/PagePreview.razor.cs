using Microsoft.AspNetCore.Components;

namespace SimpleScan.Components.Pages.Scan.Features;

public partial class PagePreview
{
    [Parameter]
    public bool HasPage { get; set; }

    [Parameter]
    public string? PreviewUrl { get; set; }
}
