namespace SimpleScan.Domain.Scanners;

public enum ScannerConnectionState
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    Busy = 3,
    Error = 4
}
