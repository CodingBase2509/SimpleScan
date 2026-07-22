using Microsoft.AspNetCore.Components;
using SimpleScan.Domain.Scanners;
using SimpleScan.Domain.Scanning;

namespace SimpleScan.Components.Pages.Scan.Features;

public partial class ScanSettingsPanel
{
    [Parameter]
    public ScanSettings? Settings { get; set; }

    [Parameter]
    public ScannerCapabilities? Capabilities { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback<ScanSettings> SettingsChanged { get; set; }

    private int SelectedDpi => Settings?.Dpi ?? SupportedDpi.First();

    private ScanColorMode SelectedColorMode => Settings?.ColorMode ?? SupportedColorModes.First();

    private string SelectedPaperSize => Settings?.PaperSize ?? SupportedPaperSizes.First();

    private string SelectedSource => Settings?.Source ?? SupportedSources.First();

    private bool SelectedDuplex => Settings?.Duplex ?? false;

    private bool SupportsDuplex => Capabilities?.SupportsDuplex == true;

    private IReadOnlyList<int> SupportedDpi =>
        Capabilities?.SupportedDpi.Count > 0
            ? Capabilities.SupportedDpi
            : [300];

    private IReadOnlyList<ScanColorMode> SupportedColorModes =>
        Capabilities?.SupportedColorModes.Count > 0
            ? Capabilities.SupportedColorModes
            : [ScanColorMode.Color];

    private IReadOnlyList<string> SupportedPaperSizes =>
        Capabilities?.SupportedPaperSizes.Count > 0
            ? Capabilities.SupportedPaperSizes
            : ["A4"];

    private IReadOnlyList<string> SupportedSources =>
        Capabilities?.SupportedSources.Count > 0
            ? Capabilities.SupportedSources
            : ["Flatbed"];

    private Task OnDpiChangedAsync(int dpi) =>
        NotifySettingsChangedAsync(dpi: dpi);

    private Task OnColorModeChangedAsync(ScanColorMode colorMode) =>
        NotifySettingsChangedAsync(colorMode: colorMode);

    private Task OnPaperSizeChangedAsync(string paperSize) =>
        NotifySettingsChangedAsync(paperSize: paperSize);

    private Task OnSourceChangedAsync(string source) =>
        NotifySettingsChangedAsync(source: source);

    private Task OnDuplexChangedAsync(bool duplex) =>
        NotifySettingsChangedAsync(duplex: duplex);

    private Task NotifySettingsChangedAsync(
        int? dpi = null,
        ScanColorMode? colorMode = null,
        string? paperSize = null,
        string? source = null,
        bool? duplex = null)
    {
        if (Settings is null || Disabled)
        {
            return Task.CompletedTask;
        }

        var settings = new ScanSettings(
            dpi ?? SelectedDpi,
            colorMode ?? SelectedColorMode,
            paperSize ?? SelectedPaperSize,
            source ?? SelectedSource,
            duplex ?? SelectedDuplex);

        if (settings.Equals(Settings))
        {
            return Task.CompletedTask;
        }

        return SettingsChanged.InvokeAsync(settings);
    }

    private static string GetColorModeLabel(ScanColorMode colorMode) =>
        colorMode switch
        {
            ScanColorMode.BlackAndWhite => "Black and white",
            ScanColorMode.Grayscale => "Grayscale",
            _ => "Color"
        };
}
