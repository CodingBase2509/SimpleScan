using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SimpleScan.ViewModels;

namespace SimpleScan.Components.Pages.Print;

public partial class PrintPage : IDisposable
{
    private const int MaxUploadCount = 50;

    [Parameter]
    public Guid DocumentId { get; set; }

    [Parameter]
    public Guid? PageId { get; set; }

    [Inject]
    public PrintPageViewModel ViewModel { get; set; } = default!;

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

    private async Task OnFilesSelectedAsync(InputFileChangeEventArgs args) =>
        await ViewModel.UploadPagesAsync(args.GetMultipleFiles(MaxUploadCount));

    private string GetUploadTriggerClass() =>
        ViewModel.CanUpload
            ? "print-page__upload-trigger"
            : "print-page__upload-trigger print-page__upload-trigger--disabled";

    private void OnViewModelStateChanged() =>
        InvokeAsync(StateHasChanged);
}
