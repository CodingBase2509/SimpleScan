using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SimpleScan.Components.Shared;

public partial class LoadingOverlay
{
    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    [Parameter]
    public Size Size { get; set; } = Size.Medium;

    [Parameter]
    public int ZIndex { get; set; } = 10;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
