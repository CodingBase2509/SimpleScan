using Microsoft.AspNetCore.Components;

namespace SimpleScan.Components.Pages.Scan.Features;

public partial class ScannerSelectionBar
{
    [Parameter]
    public string? ScannerName { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    private string SelectedScannerLabel => ScannerName ?? "Scanner";
}
