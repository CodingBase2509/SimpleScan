using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.Printing;

public sealed class PrintJob
{
    public PrintJob(
        string id,
        string printerId,
        PrintJobStatus status,
        DateTime createdAtUtc,
        string? message = null)
    {
        Id = Guard.NotWhiteSpace(id, nameof(id));
        PrinterId = Guard.NotWhiteSpace(printerId, nameof(printerId));
        Status = status;
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
    }

    public string Id { get; }

    public string PrinterId { get; }

    public PrintJobStatus Status { get; }

    public DateTime CreatedAtUtc { get; }

    public string? Message { get; }
}
