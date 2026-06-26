namespace SimpleScan.Domain.Documents;

public enum ScanDocumentStatus
{
    Draft = 0,
    Scanning = 1,
    Exporting = 2,
    Exported = 3,
    Closed = 4,
    Failed = 5
}
