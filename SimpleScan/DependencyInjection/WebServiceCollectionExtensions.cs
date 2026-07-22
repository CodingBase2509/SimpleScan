using MudBlazor.Services;
using SimpleScan.State;
using SimpleScan.ViewModels;

namespace SimpleScan.DependencyInjection;

public static class WebServiceCollectionExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddMudServices();

        services.AddScoped<ManualScannerStorage>();
        services.AddScoped<ScanWorkspaceState>();
        services.AddScoped<HomePageViewModel>();
        services.AddScoped<ScanPageViewModel>();
        services.AddScoped<PrintPageViewModel>();

        return services;
    }
}
