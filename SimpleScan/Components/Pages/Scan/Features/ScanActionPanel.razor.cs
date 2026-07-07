using Microsoft.AspNetCore.Components;

namespace SimpleScan.Components.Pages.Scan.Features;

public partial class ScanActionPanel
{
    [Parameter]
    public bool CanScan { get; set; }

    [Parameter]
    public bool CanEditPage { get; set; }

    [Parameter]
    public bool CanSave { get; set; }

    [Parameter]
    public EventCallback ScanClicked { get; set; }
}
