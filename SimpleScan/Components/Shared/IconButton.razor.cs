using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace SimpleScan.Components.Shared;

public partial class IconButton
{
    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.MoreVert;

    [Parameter]
    public string? Tooltip { get; set; }

    [Parameter]
    public Placement TooltipPlacement { get; set; } = Placement.Bottom;

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public Color Color { get; set; } = Color.Default;

    [Parameter]
    public Size Size { get; set; } = Size.Medium;

    [Parameter]
    public Variant Variant { get; set; } = Variant.Text;

    [Parameter]
    public Edge Edge { get; set; } = Edge.False;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
