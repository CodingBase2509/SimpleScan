using Microsoft.AspNetCore.Components;
using MudBlazor;
using SimpleScan.State;

namespace SimpleScan.Components.Pages.Home.Features;

public partial class AddManualScannerDialog
{
    private string? _name;
    private string? _address;
    private string? _errorMessage;

    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    private void Save()
    {
        try
        {
            var entry = ManualScannerEntry.Create(_name, _address ?? string.Empty);
            MudDialog?.Close(DialogResult.Ok(entry));
        }
        catch (ArgumentException exception)
        {
            _errorMessage = exception.Message;
        }
    }

    private void Cancel() =>
        MudDialog?.Cancel();
}
