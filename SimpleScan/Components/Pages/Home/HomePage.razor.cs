using Microsoft.AspNetCore.Components;
using SimpleScan.ViewModels;

namespace SimpleScan.Components.Pages.Home;

public partial class HomePage : IDisposable
{
    [Inject]
    public HomePageViewModel ViewModel { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ViewModel.StateChanged += OnViewModelStateChanged;
        await ViewModel.InitializeAsync();
    }

    public void Dispose()
    {
        ViewModel.StateChanged -= OnViewModelStateChanged;
    }

    private void OnViewModelStateChanged() =>
        InvokeAsync(StateHasChanged);
}
