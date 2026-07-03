using Microsoft.AspNetCore.Components;
using SimpleScan.ViewModels;

namespace SimpleScan.Components.Pages.Home;

public partial class HomePage
{
    [Inject]
    public HomePageViewModel ViewModel { get; set; } = default!;
}
