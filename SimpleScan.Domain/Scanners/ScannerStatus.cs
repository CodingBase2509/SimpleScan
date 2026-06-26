using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.Scanners;

public sealed class ScannerStatus
{
    public ScannerStatus(
        string scannerId,
        ScannerConnectionState state,
        DateTime checkedAtUtc,
        string? message = null)
    {
        ScannerId = Guard.NotWhiteSpace(scannerId, nameof(scannerId));
        State = state;
        CheckedAtUtc = Guard.Utc(checkedAtUtc, nameof(checkedAtUtc));
        Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
    }

    public string ScannerId { get; }

    public ScannerConnectionState State { get; }

    public DateTime CheckedAtUtc { get; }

    public string? Message { get; }

    public bool IsAvailable => State == ScannerConnectionState.Online;
}
