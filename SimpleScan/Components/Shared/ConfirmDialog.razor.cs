using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SimpleScan.Components.Shared;

public partial class ConfirmDialog
{
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public string ConfirmText { get; set; } = "Confirm";

    [Parameter]
    public string CancelText { get; set; } = "Cancel";

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private void Confirm() =>
        MudDialog?.Close(DialogResult.Ok(true));

    private void Cancel() =>
        MudDialog?.Cancel();
}
