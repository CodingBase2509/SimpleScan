namespace SimpleScan.Application.Events;

public static class EventTypes
{
    public static class Scanner
    {
        public const string Discovered = "scanner.discovered";
        public const string Online = "scanner.online";
        public const string Offline = "scanner.offline";
        public const string StatusChanged = "scanner.statusChanged";
        
    }

    public static class Scan
    {
        public const string Started = "scan.started";
        public const string Completed = "scan.completed";
        public const string Failed = "scan.failed";
        public const string Cancelled = "scan.cancelled";
    }

    public static class Page
    {
        public const string Scanned = "page.scanned";
        public const string PreviewReady = "page.previewReady";
        public const string ThumbnailReady = "page.thumbnailReady";
    }

    public static class Export
    {
        public const string Started = "export.started";
        public const string Completed = "export.completed";
        public const string Failed = "export.failed";
    }
}