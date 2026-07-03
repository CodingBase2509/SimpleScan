using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SimpleScan.Components.Shared;

public partial class LoadingState
{
    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    [Parameter]
    public Size Size { get; set; } = Size.Medium;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
