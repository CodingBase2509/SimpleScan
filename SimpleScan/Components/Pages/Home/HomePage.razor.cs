using Microsoft.AspNetCore.Components;
using MudBlazor;
using SimpleScan.State;
using SimpleScan.ViewModels;

namespace SimpleScan.Components.Pages.Home;

public partial class HomePage : IDisposable
{
    private IReadOnlyList<ManualScannerEntry> _manualScannerEntries = [];

    [Inject]
    public HomePageViewModel ViewModel { get; set; } = default!;

    [Inject]
    public IDialogService DialogService { get; set; } = default!;

    [Inject]
    public ManualScannerStorage ManualScannerStorage { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ViewModel.StateChanged += OnViewModelStateChanged;
        await ViewModel.InitializeAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _manualScannerEntries = await ManualScannerStorage.LoadAsync(CancellationToken.None);
        await ApplyManualScannerEntriesAsync();
    }

    public void Dispose()
    {
        ViewModel.StateChanged -= OnViewModelStateChanged;
    }

    private async Task OpenAddManualScannerDialogAsync()
    {
        var dialog = await DialogService.ShowAsync<Features.AddManualScannerDialog>("Add device");
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not ManualScannerEntry entry)
        {
            return;
        }

        _manualScannerEntries = _manualScannerEntries
            .Where(existing => existing.Id != entry.Id)
            .Append(entry)
            .OrderBy(existing => existing.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await ManualScannerStorage.SaveAsync(_manualScannerEntries, CancellationToken.None);
        await ApplyManualScannerEntriesAsync();
    }

    private async Task RemoveManualScannerAsync(string scannerId)
    {
        var entries = _manualScannerEntries
            .Where(entry => entry.Id != scannerId)
            .ToArray();

        if (entries.Length == _manualScannerEntries.Count)
        {
            return;
        }

        _manualScannerEntries = entries;

        await ManualScannerStorage.SaveAsync(_manualScannerEntries, CancellationToken.None);
        await ApplyManualScannerEntriesAsync();
    }

    private async Task ApplyManualScannerEntriesAsync() =>
        await ViewModel.SetManualDevicesAsync(
            _manualScannerEntries
                .Select(entry => entry.ToScannerDevice())
                .ToArray(),
            CancellationToken.None);

    private void OnViewModelStateChanged() =>
        InvokeAsync(StateHasChanged);
}
