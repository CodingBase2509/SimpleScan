namespace SimpleScan.Infrastructure.Options;

public class FileStoreOptions
{
    public const string SectionName = "FileStore";

    public string BasePath { get; set; } = "/data/scan-documents";
}
