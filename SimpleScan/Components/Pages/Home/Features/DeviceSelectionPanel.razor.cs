using Microsoft.AspNetCore.Components;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Components.Pages.Home.Features;

public partial class DeviceSelectionPanel
{
    [Parameter]
    public IReadOnlyList<ScannerDevice> Devices { get; set; } = [];

    [Parameter]
    public string? SelectedDeviceId { get; set; }

    [Parameter]
    public string DiscoveryStatusLabel { get; set; } = "Discovery idle";

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public string? ErrorMessage { get; set; }

    [Parameter]
    public EventCallback<string> SelectedDeviceIdChanged { get; set; }

    [Parameter]
    public EventCallback<string> DeleteDevice { get; set; }
}
