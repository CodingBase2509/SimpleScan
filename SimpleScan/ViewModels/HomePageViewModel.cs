using Microsoft.AspNetCore.Components;
using SimpleScan.Application.Scanners;
using SimpleScan.Application.Scanning;
using SimpleScan.Domain.Scanners;
using SimpleScan.Domain.Scanning;

namespace SimpleScan.ViewModels;

public sealed class HomePageViewModel(
    ScannerService scannerService,
    ScanDocumentService scanDocumentService,
    NavigationManager navigationManager)
{
    public event Action? StateChanged;

    private IReadOnlyList<ScannerDevice> _discoveredDevices = [];

    private IReadOnlyList<ScannerDevice> _manualDevices = [];

    public IReadOnlyList<ScannerDevice> Devices { get; private set; } = [];

    public string? SelectedDeviceId { get; private set; }

    public string DiscoveryStatusLabel { get; private set; } = "Discovery idle";

    public string? ErrorMessage { get; private set; }

    public bool IsLoadingDevices { get; private set; }

    public bool IsStartingScan { get; private set; }

    public bool CanStartScan =>
        !IsLoadingDevices &&
        !IsStartingScan &&
        SelectedDevice?.CanScan == true;

    private ScannerDevice? SelectedDevice =>
        Devices.FirstOrDefault(device => device.Id == SelectedDeviceId);

    public async Task InitializeAsync()
    {
        if (_discoveredDevices.Count == 0)
        {
            await RefreshDevicesAsync();
        }
    }

    public async Task SetManualDevicesAsync(
        IReadOnlyList<ScannerDevice> manualDevices,
        CancellationToken cancellationToken)
    {
        _manualDevices = manualDevices;

        foreach (var device in manualDevices)
        {
            await scannerService.RememberAsync(device, cancellationToken);
        }

        MergeDevices();
        SelectedDeviceId = PreserveSelectionOrSelectFirst(SelectedDeviceId, Devices);
        NotifyStateChanged();
    }

    public async Task RefreshDevicesAsync()
    {
        IsLoadingDevices = true;
        ErrorMessage = null;
        DiscoveryStatusLabel = "Discovering";
        NotifyStateChanged();

        try
        {
            var scanners = await scannerService.DiscoverAsync(CancellationToken.None);
            _discoveredDevices = scanners;
            MergeDevices();
            SelectedDeviceId = PreserveSelectionOrSelectFirst(SelectedDeviceId, Devices);
            DiscoveryStatusLabel = Devices.Count == 0 ? "No devices" : $"{Devices.Count} found";
        }
        catch (Exception exception)
        {
            _discoveredDevices = [];
            MergeDevices();
            SelectedDeviceId = PreserveSelectionOrSelectFirst(SelectedDeviceId, Devices);
            ErrorMessage = exception.Message;
            DiscoveryStatusLabel = "Discovery failed";
        }
        finally
        {
            IsLoadingDevices = false;
            NotifyStateChanged();
        }
    }

    public void SelectDevice(string deviceId)
    {
        if (Devices.Any(device => device.Id == deviceId))
        {
            SelectedDeviceId = deviceId;
            NotifyStateChanged();
        }
    }

    public async Task StartScanAsync()
    {
        var selectedDevice = SelectedDevice;
        if (selectedDevice is null || !selectedDevice.CanScan)
        {
            return;
        }

        IsStartingScan = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var capabilities = await scannerService.GetCapabilitiesAsync(
                selectedDevice.Id,
                refresh: false,
                CancellationToken.None);

            var settings = CreateInitialScanSettings(capabilities);
            var document = await scanDocumentService.CreateAsync(
                selectedDevice.Id,
                settings,
                name: null,
                CancellationToken.None);

            navigationManager.NavigateTo($"/scan/{document.Id}");
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsStartingScan = false;
            NotifyStateChanged();
        }
    }

    private static string? PreserveSelectionOrSelectFirst(
        string? selectedDeviceId,
        IReadOnlyList<ScannerDevice> devices)
    {
        if (selectedDeviceId is not null &&
            devices.Any(device => device.Id == selectedDeviceId))
        {
            return selectedDeviceId;
        }

        return devices.FirstOrDefault()?.Id;
    }

    private static ScanSettings CreateInitialScanSettings(ScannerCapabilities capabilities)
    {
        var dpi = capabilities.SupportedDpi.Contains(300)
            ? 300
            : capabilities.SupportedDpi.FirstOrDefault(300);

        var colorMode = capabilities.SupportedColorModes.Contains(ScanColorMode.Color)
            ? ScanColorMode.Color
            : capabilities.SupportedColorModes.FirstOrDefault(ScanColorMode.Color);

        var paperSize = capabilities.SupportedPaperSizes.Contains("A4", StringComparer.OrdinalIgnoreCase)
            ? "A4"
            : capabilities.SupportedPaperSizes.FirstOrDefault() ?? "A4";

        var source = capabilities.SupportedSources.Contains("Flatbed", StringComparer.OrdinalIgnoreCase)
            ? "Flatbed"
            : capabilities.SupportedSources.FirstOrDefault() ?? "Flatbed";

        return new ScanSettings(dpi, colorMode, paperSize, source, duplex: false);
    }

    private void MergeDevices()
    {
        Devices = _discoveredDevices
            .Concat(_manualDevices)
            .GroupBy(device => device.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(device => device.Protocol == ScannerProtocol.Mock)
            .ThenBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void NotifyStateChanged() =>
        StateChanged?.Invoke();
}
