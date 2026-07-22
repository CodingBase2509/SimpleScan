using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SimpleScan.Infrastructure.Options;

namespace SimpleScan.Infrastructure.FileStorage;

public sealed class FileStoreStartupCleanupService(IOptions<FileStoreOptions> options) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var basePath = NormalizeBasePath(options.Value.BasePath);
        Directory.CreateDirectory(basePath);

        foreach (var file in Directory.EnumerateFiles(basePath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(file);
        }

        foreach (var directory in Directory.EnumerateDirectories(basePath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.Delete(directory, recursive: true);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    private static string NormalizeBasePath(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException("File store base path cannot be empty.", nameof(basePath));
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(basePath.Trim()));
    }
}
