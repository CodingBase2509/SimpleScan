using Microsoft.AspNetCore.Components;
using SimpleScan.ViewModels;

namespace SimpleScan.Components.Pages.Scan;

public partial class ScanPage : IDisposable
{
    [Parameter]
    public Guid DocumentId { get; set; }
    
    [Parameter]
    public Guid? PageId { get; set; }
    
    [Inject]
    public ScanPageViewModel ViewModel { get; set; } = default!;
    
    protected override async Task OnParametersSetAsync()
    {
        ViewModel.StateChanged -= OnViewModelStateChanged;
        ViewModel.StateChanged += OnViewModelStateChanged;
        await ViewModel.InitializeAsync(DocumentId, PageId);
    }

    public void Dispose()
    {
        ViewModel.StateChanged -= OnViewModelStateChanged;
    }

    private void OnViewModelStateChanged() =>
        InvokeAsync(StateHasChanged);
}
