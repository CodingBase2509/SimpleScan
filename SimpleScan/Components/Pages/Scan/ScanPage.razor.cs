using Microsoft.AspNetCore.Components;

namespace SimpleScan.Components.Pages.Scan;

public partial class ScanPage
{
    [Parameter]
    public Guid DocumentId { get; set; }
}
