using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SimpleScan.Components.Pages.Home.Features;

public partial class HomeActionBar
{
    [Parameter]
    public bool CanScan { get; set; }

    [Parameter]
    public bool IsStartingScan { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> StartScanClicked { get; set; }
}
