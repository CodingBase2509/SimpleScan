using SimpleScan.Domain.Common;
using SimpleScan.Domain.PageEditing;

namespace SimpleScan.Domain.ScanDocuments;

public sealed class ScannedPage
{
    public ScannedPage(
        Guid id,
        int pageNumber,
        string originalFilePath,
        DateTime scannedAtUtc,
        string? renderedFilePath = null,
        string? thumbnailFilePath = null,
        PageEditSettings? editSettings = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Page id cannot be empty.", nameof(id));
        }

        Id = id;
        PageNumber = Guard.Positive(pageNumber, nameof(pageNumber));
        OriginalFilePath = Guard.NotWhiteSpace(originalFilePath, nameof(originalFilePath));
        RenderedFilePath = string.IsNullOrWhiteSpace(renderedFilePath) ? null : renderedFilePath.Trim();
        ThumbnailFilePath = string.IsNullOrWhiteSpace(thumbnailFilePath) ? null : thumbnailFilePath.Trim();
        EditSettings = editSettings ?? PageEditSettings.None;
        ScannedAtUtc = Guard.Utc(scannedAtUtc, nameof(scannedAtUtc));
    }

    public Guid Id { get; }

    public int PageNumber { get; private set; }

    public string OriginalFilePath { get; }

    public string? RenderedFilePath { get; private set; }

    public string? ThumbnailFilePath { get; private set; }

    public PageEditSettings EditSettings { get; private set; }

    public DateTime ScannedAtUtc { get; }

    public static ScannedPage CreateNew(int pageNumber, string originalFilePath, DateTime scannedAtUtc) =>
        new(Guid.NewGuid(), pageNumber, originalFilePath, scannedAtUtc);

    internal void SetPageNumber(int pageNumber) =>
        PageNumber = Guard.Positive(pageNumber, nameof(pageNumber));

    public void SetRenderedFilePath(string renderedFilePath) =>
        RenderedFilePath = Guard.NotWhiteSpace(renderedFilePath, nameof(renderedFilePath));

    public void SetThumbnailFilePath(string thumbnailFilePath) =>
        ThumbnailFilePath = Guard.NotWhiteSpace(thumbnailFilePath, nameof(thumbnailFilePath));

    public void UpdateEditSettings(PageEditSettings editSettings) =>
        EditSettings = editSettings ?? throw new ArgumentNullException(nameof(editSettings));

    public void ResetEditSettings() =>
        EditSettings = PageEditSettings.None;
}
