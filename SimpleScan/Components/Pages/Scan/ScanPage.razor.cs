using Microsoft.AspNetCore.Components;
using SimpleScan.ViewModels;

namespace SimpleScan.Components.Pages.Scan;

public partial class ScanPage
{
    [Parameter]
    public Guid DocumentId { get; set; }
    
    [Parameter]
    public Guid? PageId { get; set; }
    
    [Inject]
    public ScanPageViewModel ViewModel { get; set; } = default!;
}
