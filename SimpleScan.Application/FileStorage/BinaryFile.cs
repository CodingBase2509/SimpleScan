namespace SimpleScan.Application.FileStorage;

public sealed class BinaryFile
{
    public BinaryFile(
        Stream content,
        string fileName,
        string contentType,
        long? length = null)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));

        FileName = string.IsNullOrWhiteSpace(fileName)
            ? throw new ArgumentException("File name cannot be empty.", nameof(fileName))
            : fileName.Trim();

        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? throw new ArgumentException("Content type cannot be empty.", nameof(contentType))
            : contentType.Trim();

        if (length is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Length cannot be negative.");
        }

        Length = length;
    }

    public Stream Content { get; }

    public string FileName { get; }

    public string ContentType { get; }

    public long? Length { get; }
}
