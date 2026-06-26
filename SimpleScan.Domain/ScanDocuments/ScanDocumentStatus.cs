namespace SimpleScan.Domain.ScanDocuments;

public enum ScanDocumentStatus
{
    Draft = 0,
    Scanning = 1,
    Editing = 2,
    Exporting = 3,
    Exported = 4,
    Closed = 5,
    Failed = 6
}
