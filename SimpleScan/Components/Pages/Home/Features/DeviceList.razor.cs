using Microsoft.AspNetCore.Components;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Components.Pages.Home.Features;

public partial class DeviceList
{
    [Parameter]
    public List<ScannerDevice> Devices { get; set; } = [];
    
    [Parameter]
    public ScannerDevice? SelectedDevice { get; set; }
    
    [Parameter]
    public EventCallback<ScannerDevice> SelectedDeviceChanged { get; set; }
}
