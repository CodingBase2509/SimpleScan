namespace SimpleScan.Application.Printing;

public sealed record PrintFileInput(
    Guid PageId,
    int PageNumber,
    string FilePath,
    string FileName,
    string ContentType);
