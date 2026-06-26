namespace SimpleScan.Application.FileStorage;

public sealed class StoredFile
{
    public StoredFile(
        string path,
        string fileName,
        string contentType,
        long length)
    {
        Path = string.IsNullOrWhiteSpace(path)
            ? throw new ArgumentException("Path cannot be empty.", nameof(path))
            : path.Trim();

        FileName = string.IsNullOrWhiteSpace(fileName)
            ? throw new ArgumentException("File name cannot be empty.", nameof(fileName))
            : fileName.Trim();

        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? throw new ArgumentException("Content type cannot be empty.", nameof(contentType))
            : contentType.Trim();

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Length cannot be negative.");
        }

        Length = length;
    }

    public string Path { get; }

    public string FileName { get; }

    public string ContentType { get; }

    public long Length { get; }
}
