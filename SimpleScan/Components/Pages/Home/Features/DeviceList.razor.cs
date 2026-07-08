using Microsoft.AspNetCore.Components;
using MudBlazor;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Components.Pages.Home.Features;

public partial class DeviceList
{
    [Parameter]
    public IReadOnlyList<ScannerDevice> Devices { get; set; } = [];
    
    [Parameter]
    public string? SelectedDeviceId { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public string? ErrorMessage { get; set; }
    
    [Parameter]
    public EventCallback<string> SelectedDeviceIdChanged { get; set; }

    [Parameter]
    public EventCallback<string> DeleteDevice { get; set; }

    private Task SelectDeviceAsync(string deviceId) =>
        SelectedDeviceIdChanged.InvokeAsync(deviceId);

    private Task DeleteDeviceAsync(string deviceId) =>
        DeleteDevice.InvokeAsync(deviceId);

    private string GetItemClass(ScannerDevice device) =>
        device.Id == SelectedDeviceId
            ? "device-list__item device-list__item--selected"
            : "device-list__item";

    private static string GetDeviceIcon(ScannerDevice device) =>
        device.Protocol == ScannerProtocol.Unknown
            ? Icons.Material.Filled.Devices
            : Icons.Material.Filled.DocumentScanner;

    private static bool IsManualDevice(ScannerDevice device) =>
        string.Equals(device.Manufacturer, "Manual", StringComparison.Ordinal);
}
