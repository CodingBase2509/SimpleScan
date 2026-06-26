using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.Downloads;

public sealed class DownloadTicket
{
    public DownloadTicket(
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

        Token = Guard.NotWhiteSpace(token, nameof(token));
        DocumentId = documentId;
        FilePath = Guard.NotWhiteSpace(filePath, nameof(filePath));
        FileName = Guard.NotWhiteSpace(fileName, nameof(fileName));
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        ExpiresAtUtc = Guard.Utc(expiresAtUtc, nameof(expiresAtUtc));

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
