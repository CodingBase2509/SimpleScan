using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SimpleScan.Components.Shared;

public partial class InlineError
{
    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public Severity Severity { get; set; } = Severity.Error;

    [Parameter]
    public Variant Variant { get; set; } = Variant.Text;

    [Parameter]
    public bool Dense { get; set; } = true;

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.ErrorOutline;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
