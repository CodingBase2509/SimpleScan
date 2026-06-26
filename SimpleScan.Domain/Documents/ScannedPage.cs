using SimpleScan.Domain.Common;
using SimpleScan.Domain.PageEditing;

namespace SimpleScan.Domain.Documents;

public sealed class ScannedPage
{
    public ScannedPage(
        Guid id,
        int pageNumber,
        string originalPath,
        DateTime scannedAtUtc,
        string? previewPath = null,
        string? thumbnailPath = null,
        PageEditSettings? editSettings = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Page id cannot be empty.", nameof(id));
        }

        Id = id;
        PageNumber = Guard.Positive(pageNumber, nameof(pageNumber));
        OriginalPath = Guard.NotWhiteSpace(originalPath, nameof(originalPath));
        PreviewPath = string.IsNullOrWhiteSpace(previewPath) ? null : previewPath.Trim();
        ThumbnailPath = string.IsNullOrWhiteSpace(thumbnailPath) ? null : thumbnailPath.Trim();
        EditSettings = editSettings ?? PageEditSettings.None;
        ScannedAtUtc = Guard.Utc(scannedAtUtc, nameof(scannedAtUtc));
    }

    public Guid Id { get; }

    public int PageNumber { get; private set; }

    public string OriginalPath { get; }

    public string? PreviewPath { get; private set; }

    public string? ThumbnailPath { get; private set; }

    public PageEditSettings EditSettings { get; private set; }

    public DateTime ScannedAtUtc { get; }

    public static ScannedPage CreateNew(int pageNumber, string originalPath, DateTime scannedAtUtc) =>
        new(Guid.NewGuid(), pageNumber, originalPath, scannedAtUtc);

    internal void SetPageNumber(int pageNumber) =>
        PageNumber = Guard.Positive(pageNumber, nameof(pageNumber));

    public void SetPreviewPath(string previewPath) =>
        PreviewPath = Guard.NotWhiteSpace(previewPath, nameof(previewPath));

    public void SetThumbnailPath(string thumbnailPath) =>
        ThumbnailPath = Guard.NotWhiteSpace(thumbnailPath, nameof(thumbnailPath));

    public void UpdateEditSettings(PageEditSettings editSettings) =>
        EditSettings = editSettings ?? throw new ArgumentNullException(nameof(editSettings));

    public void ResetEditSettings() =>
        EditSettings = PageEditSettings.None;
}
