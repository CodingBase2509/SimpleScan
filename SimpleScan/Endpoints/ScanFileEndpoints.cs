using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Scanning;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Documents;

namespace SimpleScan.Endpoints;

public static class ScanFileEndpoints
{
    public static IEndpointRouteBuilder MapScanFileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/scan-files");

        group.MapGet(
            "/documents/{documentId:guid}/pages/{pageId:guid}/preview",
            GetPreviewAsync);

        group.MapGet(
            "/documents/{documentId:guid}/pages/{pageId:guid}/thumbnail",
            GetThumbnailAsync);

        group.MapGet(
            "/documents/{documentId:guid}/pages/{pageId:guid}/original",
            GetOriginalAsync);

        return endpoints;
    }

    private static Task<IResult> GetPreviewAsync(
        Guid documentId,
        Guid pageId,
        ScanDocumentService documents,
        IScanFileStorage files,
        CancellationToken cancellationToken) =>
        GetPageFileAsync(
            documentId,
            pageId,
            documents,
            files,
            page => page.PreviewPath,
            cancellationToken);

    private static Task<IResult> GetThumbnailAsync(
        Guid documentId,
        Guid pageId,
        ScanDocumentService documents,
        IScanFileStorage files,
        CancellationToken cancellationToken) =>
        GetPageFileAsync(
            documentId,
            pageId,
            documents,
            files,
            page => page.ThumbnailPath,
            cancellationToken);

    private static Task<IResult> GetOriginalAsync(
        Guid documentId,
        Guid pageId,
        ScanDocumentService documents,
        IScanFileStorage files,
        CancellationToken cancellationToken) =>
        GetPageFileAsync(
            documentId,
            pageId,
            documents,
            files,
            page => page.OriginalPath,
            cancellationToken);

    private static async Task<IResult> GetPageFileAsync(
        Guid documentId,
        Guid pageId,
        ScanDocumentService documents,
        IScanFileStorage files,
        Func<ScannedPage, string?> getPath,
        CancellationToken cancellationToken)
    {
        string? path;

        try
        {
            var document = await documents.GetAsync(documentId, cancellationToken);
            var page = document.GetPage(pageId);
            path = getPath(page);
        }
        catch (DomainException)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(path) || !await files.ExistsAsync(path, cancellationToken))
        {
            return Results.NotFound();
        }

        var stream = await files.OpenReadAsync(path, cancellationToken);
        return Results.File(stream, GetContentType(path));
    }

    private static string GetContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".tif" or ".tiff" => "image/tiff",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".rtf" => "application/rtf",
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".odt" => "application/vnd.oasis.opendocument.text",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".odp" => "application/vnd.oasis.opendocument.presentation",
            _ => "application/octet-stream"
        };
    }
}
