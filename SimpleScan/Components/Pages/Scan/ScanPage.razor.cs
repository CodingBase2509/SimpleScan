using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;
    
    protected override async Task OnParametersSetAsync()
    {
        ViewModel.StateChanged -= OnViewModelStateChanged;
        ViewModel.DownloadRequested -= OnDownloadRequestedAsync;
        ViewModel.StateChanged += OnViewModelStateChanged;
        ViewModel.DownloadRequested += OnDownloadRequestedAsync;
        await ViewModel.InitializeAsync(DocumentId, PageId);
    }

    public void Dispose()
    {
        ViewModel.StateChanged -= OnViewModelStateChanged;
        ViewModel.DownloadRequested -= OnDownloadRequestedAsync;
    }

    private void OnViewModelStateChanged() =>
        InvokeAsync(StateHasChanged);

    private async Task OnDownloadRequestedAsync(string url) =>
        await JSRuntime.InvokeVoidAsync("simpleScan.downloadFile", url);
}
