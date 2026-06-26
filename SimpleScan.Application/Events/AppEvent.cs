namespace SimpleScan.Application.Events;

public sealed class AppEvent
{
    public AppEvent(
        string type,
        DateTime createdAtUtc,
        string? scannerId = null,
        Guid? documentId = null,
        Guid? pageId = null,
        object? payload = null)
    {
        Type = string.IsNullOrWhiteSpace(type)
            ? throw new ArgumentException("Event type cannot be empty.", nameof(type))
            : type.Trim();

        CreatedAtUtc = createdAtUtc.Kind == DateTimeKind.Utc
            ? createdAtUtc
            : DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc);

        ScannerId = string.IsNullOrWhiteSpace(scannerId) ? null : scannerId.Trim();
        DocumentId = documentId;
        PageId = pageId;
        Payload = payload;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string Type { get; }

    public DateTime CreatedAtUtc { get; }

    public string? ScannerId { get; }

    public Guid? DocumentId { get; }

    public Guid? PageId { get; }

    public object? Payload { get; }
}