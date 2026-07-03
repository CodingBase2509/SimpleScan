namespace SimpleScan.Infrastructure.FileStorage;

internal static class FileUtils
{
    public static string GetDocFolder(string basePath, Guid docId)
    {
        return Path.Combine(basePath, docId.ToString());
    }

    public static string GetOriginalFolder(string basePath, Guid docId) =>
        Path.Combine(GetDocFolder(basePath, docId), "original");

    public static string GetPreviewFolder(string basePath, Guid docId) =>
        Path.Combine(GetDocFolder(basePath, docId), "preview");

    public static string GetThumbnailFolder(string basePath, Guid docId) =>
        Path.Combine(GetDocFolder(basePath, docId), "thumbnails");

    public static string GetExportFolder(string basePath, Guid docId) =>
        Path.Combine(GetDocFolder(basePath, docId), "export");
}
