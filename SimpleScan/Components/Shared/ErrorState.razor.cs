using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace SimpleScan.Components.Shared;

public partial class ErrorState
{
    [Parameter]
    public string Title { get; set; } = "Something went wrong";

    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public string RetryText { get; set; } = "Retry";

    [Parameter]
    public Severity Severity { get; set; } = Severity.Error;

    [Parameter]
    public Variant Variant { get; set; } = Variant.Outlined;

    [Parameter]
    public EventCallback<MouseEventArgs> OnRetry { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
