using Microsoft.AspNetCore.Components;
using SimpleScan.ViewModels;

namespace SimpleScan.Components.Pages.Scan.Features;

public partial class PagePreview
{
    [Parameter]
    public bool HasPage { get; set; }

    [Parameter]
    public string? PreviewUrl { get; set; }

    [Parameter]
    public PagePreviewKind PreviewKind { get; set; } = PagePreviewKind.Image;

    [Parameter]
    public string? FileName { get; set; }

    private string DisplayFileName =>
        string.IsNullOrWhiteSpace(FileName) ? "Selected file" : FileName;
}
