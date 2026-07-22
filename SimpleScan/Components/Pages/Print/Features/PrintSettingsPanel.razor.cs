using Microsoft.AspNetCore.Components;
using SimpleScan.Domain.Printing;

namespace SimpleScan.Components.Pages.Print.Features;

public partial class PrintSettingsPanel
{
    [Parameter]
    public PrintSettings Settings { get; set; } = new();

    [Parameter]
    public PrinterCapabilities? Capabilities { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback<PrintSettings> SettingsChanged { get; set; }

    private int SelectedCopies => Math.Min(Settings.Copies, MaxCopies);

    private string SelectedPaperSize => Settings.PaperSize;

    private PrintColorMode SelectedColorMode => Settings.ColorMode;

    private PrintDuplexMode SelectedDuplexMode => Settings.DuplexMode;

    private int MaxCopies => Capabilities?.MaxCopies ?? 99;

    private IReadOnlyList<int> SupportedCopyCounts =>
        Enumerable.Range(1, Math.Min(MaxCopies, 20)).ToArray();

    private IReadOnlyList<string> SupportedPaperSizes =>
        Capabilities?.SupportedPaperSizes.Count > 0
            ? Capabilities.SupportedPaperSizes
            : ["A4", "Letter"];

    private IReadOnlyList<PrintColorMode> SupportedColorModes =>
        Capabilities?.SupportedColorModes.Count > 0
            ? Capabilities.SupportedColorModes
            : [PrintColorMode.Color, PrintColorMode.Grayscale];

    private IReadOnlyList<PrintDuplexMode> SupportedDuplexModes =>
        Capabilities?.SupportsDuplex == true
            ? [PrintDuplexMode.OneSided, PrintDuplexMode.DuplexLongEdge, PrintDuplexMode.DuplexShortEdge]
            : [PrintDuplexMode.OneSided];

    private Task OnCopiesChangedAsync(int copies) =>
        NotifySettingsChangedAsync(copies: copies);

    private Task OnPaperSizeChangedAsync(string paperSize) =>
        NotifySettingsChangedAsync(paperSize: paperSize);

    private Task OnColorModeChangedAsync(PrintColorMode colorMode) =>
        NotifySettingsChangedAsync(colorMode: colorMode);

    private Task OnDuplexModeChangedAsync(PrintDuplexMode duplexMode) =>
        NotifySettingsChangedAsync(duplexMode: duplexMode);

    private Task NotifySettingsChangedAsync(
        int? copies = null,
        string? paperSize = null,
        PrintColorMode? colorMode = null,
        PrintDuplexMode? duplexMode = null)
    {
        if (Disabled)
        {
            return Task.CompletedTask;
        }

        var settings = new PrintSettings(
            copies ?? SelectedCopies,
            paperSize ?? SelectedPaperSize,
            colorMode ?? SelectedColorMode,
            duplexMode ?? SelectedDuplexMode);

        return SettingsChanged.InvokeAsync(settings);
    }

    private static string GetColorModeLabel(PrintColorMode colorMode) =>
        colorMode == PrintColorMode.Grayscale ? "Grayscale" : "Color";

    private static string GetDuplexModeLabel(PrintDuplexMode duplexMode) =>
        duplexMode switch
        {
            PrintDuplexMode.DuplexLongEdge => "Two-sided long edge",
            PrintDuplexMode.DuplexShortEdge => "Two-sided short edge",
            _ => "One-sided"
        };
}
