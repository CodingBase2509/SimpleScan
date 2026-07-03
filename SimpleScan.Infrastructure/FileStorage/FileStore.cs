using SimpleScan.Application.FileStorage;
using SimpleScan.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace SimpleScan.Infrastructure.FileStorage;

public sealed class FileStore : IScanFileStorage
{
    private readonly string _basePath;

    public FileStore(IOptions<FileStoreOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _basePath = NormalizeBasePath(options.Value.BasePath);
    }

    public Task<StoredFile> SaveOriginalPageAsync(
        Guid documentId,
        int pageNumber,
        BinaryFile file,
        CancellationToken cancellationToken) =>
        SavePageFileAsync(FileUtils.GetOriginalFolder(_basePath, documentId), pageNumber, file, cancellationToken);

    public Task<StoredFile> SavePreviewAsync(
        Guid documentId,
        int pageNumber,
        BinaryFile file,
        CancellationToken cancellationToken) =>
        SavePageFileAsync(FileUtils.GetPreviewFolder(_basePath, documentId), pageNumber, file, cancellationToken);

    public Task<StoredFile> SaveThumbnailAsync(
        Guid documentId,
        int pageNumber,
        BinaryFile file,
        CancellationToken cancellationToken) =>
        SavePageFileAsync(FileUtils.GetThumbnailFolder(_basePath, documentId), pageNumber, file, cancellationToken);

    public Task<StoredFile> SavePdfExportAsync(
        Guid documentId,
        BinaryFile file,
        CancellationToken cancellationToken) =>
        SaveFileAsync(FileUtils.GetExportFolder(_basePath, documentId), file, file.FileName, cancellationToken);

    public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var safePath = EnsurePathIsInsideBasePath(path);

        if (!File.Exists(safePath))
        {
            throw new FileNotFoundException("Stored file was not found.", safePath);
        }

        Stream stream = new FileStream(
            safePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var safePath = EnsurePathIsInsideBasePath(path);
        return Task.FromResult(File.Exists(safePath));
    }

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var safePath = EnsurePathIsInsideBasePath(path);

        if (!File.Exists(safePath))
        {
            return Task.CompletedTask;
        }

        File.Delete(safePath);
        return Task.CompletedTask;
    }

    public Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var documentFolder = FileUtils.GetDocFolder(_basePath, documentId);

        if (Directory.Exists(documentFolder))
        {
            Directory.Delete(documentFolder, recursive: true);
        }

        return Task.CompletedTask;
    }

    private Task<StoredFile> SavePageFileAsync(string folderPath, int pageNumber, BinaryFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "Page number must be greater than zero.");
        }

        var extension = GetExtension(file);
        var fileName = $"page-{pageNumber:000}{extension}";
        return SaveFileAsync(folderPath, file, fileName, cancellationToken);
    }

    private async Task<StoredFile> SaveFileAsync(
        string folderPath,
        BinaryFile file,
        string fileName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        cancellationToken.ThrowIfCancellationRequested();

        var safeFolderPath = EnsurePathIsInsideBasePath(folderPath);
        Directory.CreateDirectory(safeFolderPath);

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        }

        var filePath = GetAvailableFilePath(safeFolderPath, safeFileName);

        await using (var output = new FileStream(
                         filePath,
                         FileMode.CreateNew,
                         FileAccess.Write,
                         FileShare.None,
                         bufferSize: 81920,
                         useAsync: true))
        {
            if (file.Content.CanSeek)
            {
                file.Content.Position = 0;
            }

            await file.Content.CopyToAsync(output, cancellationToken);
        }

        var fileInfo = new FileInfo(filePath);
        return new StoredFile(fileInfo.FullName, fileInfo.Name, file.ContentType, fileInfo.Length);
    }

    private string GetAvailableFilePath(string folderPath, string fileName)
    {
        var filePath = EnsurePathIsInsideBasePath(Path.Combine(folderPath, fileName));
        if (!File.Exists(filePath))
        {
            return filePath;
        }

        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);

        for (var attempt = 1; attempt <= 100; attempt++)
        {
            var candidateName = $"{name}-{Guid.NewGuid():N}{extension}";
            var candidatePath = EnsurePathIsInsideBasePath(Path.Combine(folderPath, candidateName));

            if (!File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        throw new IOException($"Could not create a unique file name for '{fileName}'.");
    }

    private string EnsurePathIsInsideBasePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        var fullPath = Path.GetFullPath(path);

        if (!fullPath.Equals(_basePath, StringComparison.Ordinal)
            && !fullPath.StartsWith(_basePath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Path is outside the configured scan file storage base path.");
        }

        return fullPath;
    }

    private static string NormalizeBasePath(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException("File store base path cannot be empty.", nameof(basePath));
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(basePath.Trim()));
    }

    private static string GetExtension(BinaryFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!string.IsNullOrWhiteSpace(extension))
        {
            return extension;
        }

        return file.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/svg+xml" => ".svg",
            "image/tiff" => ".tiff",
            "application/pdf" => ".pdf",
            _ => ".bin"
        };
    }
}
