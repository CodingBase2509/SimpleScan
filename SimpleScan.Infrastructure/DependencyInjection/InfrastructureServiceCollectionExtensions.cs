using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleScan.Application.Downloads;
using SimpleScan.Application.Events;
using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Processing.Images;
using SimpleScan.Application.Scanners;
using SimpleScan.Application.Scanning;
using SimpleScan.Application.Stores;
using SimpleScan.Infrastructure.Events;
using SimpleScan.Infrastructure.FileStorage;
using SimpleScan.Infrastructure.Options;
using SimpleScan.Infrastructure.Processing.Images;
using SimpleScan.Infrastructure.Scanning;
using SimpleScan.Infrastructure.Stores;

namespace SimpleScan.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FileStoreOptions>(
            configuration.GetSection(FileStoreOptions.SectionName));

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<ScanDocumentService>();
        services.AddScoped<ScanPageService>();
        services.AddScoped<ScannerService>();
        services.AddScoped<DownloadTicketService>();

        services.AddSingleton<IScanFileStorage, FileStore>();
        services.AddSingleton<IImageProcessor, ImageSharpImageProcessor>();
        services.AddSingleton<IScanDocumentStore, InMemoryScanDocumentStore>();
        services.AddSingleton<IScannerStore, InMemoryScannerStore>();
        services.AddSingleton<IDownloadTicketStore, InMemoryDownloadStore>();

        services.AddSingleton<InMemoryEventBus>();
        services.AddSingleton<IEventPublisher>(provider => provider.GetRequiredService<InMemoryEventBus>());
        services.AddSingleton<IEventSubscriber>(provider => provider.GetRequiredService<InMemoryEventBus>());

        services.AddSingleton<IScannerProvider, MockScannerProvider>();

        return services;
    }
}
