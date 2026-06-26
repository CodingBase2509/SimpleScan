namespace SimpleScan.Application.Processing.Pdf;

public sealed class PdfPageInput
{
    public PdfPageInput(
        Guid pageId,
        int pageNumber,
        Stream imageContent,
        string contentType)
    {
        if (pageId == Guid.Empty)
        {
            throw new ArgumentException("Page id cannot be empty.", nameof(pageId));
        }

        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "Page number must be greater than zero.");
        }

        PageId = pageId;
        PageNumber = pageNumber;
        ImageContent = imageContent ?? throw new ArgumentNullException(nameof(imageContent));
        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? throw new ArgumentException("Content type cannot be empty.", nameof(contentType))
            : contentType.Trim();
    }

    public Guid PageId { get; }

    public int PageNumber { get; }

    public Stream ImageContent { get; }

    public string ContentType { get; }
}
