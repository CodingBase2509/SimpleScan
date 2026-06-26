using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.Downloads;

public sealed class DownloadToken
{
    public DownloadToken(
        string token,
        Guid documentId,
        string filePath,
        string fileName,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (documentId == Guid.Empty)
        {
            throw new ArgumentException("Document id cannot be empty.", nameof(documentId));
        }

        Token = Guard.NotWhiteSpace(token);
        DocumentId = documentId;
        FilePath = Guard.NotWhiteSpace(filePath);
        FileName = Guard.NotWhiteSpace(fileName);
        CreatedAtUtc = Guard.Utc(createdAtUtc);
        ExpiresAtUtc = Guard.Utc(expiresAtUtc);

        if (ExpiresAtUtc <= CreatedAtUtc)
        {
            throw new ArgumentException("Expiration must be after creation time.", nameof(expiresAtUtc));
        }
    }

    public string Token { get; }

    public Guid DocumentId { get; }

    public string FilePath { get; }

    public string FileName { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime ExpiresAtUtc { get; }

    public bool IsExpired(DateTime utcNow) =>
        Guard.Utc(utcNow, nameof(utcNow)) >= ExpiresAtUtc;
}
