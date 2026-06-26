using SimpleScan.Domain.Common;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Domain.ScanDocuments;

public sealed class ScanDocument
{
    private readonly List<ScannedPage> _pages;

    public ScanDocument(
        Guid id,
        string scannerId,
        ScanSettings settings,
        DateTime createdAtUtc,
        string? name = null,
        ScanDocumentStatus status = ScanDocumentStatus.Draft,
        IEnumerable<ScannedPage>? pages = null,
        DateTime? closedAtUtc = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Document id cannot be empty.", nameof(id));
        }

        Id = id;
        ScannerId = Guard.NotWhiteSpace(scannerId, nameof(scannerId));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        Status = status;
        ClosedAtUtc = closedAtUtc.HasValue ? Guard.Utc(closedAtUtc.Value, nameof(closedAtUtc)) : null;
        _pages = pages?.OrderBy(page => page.PageNumber).ToList() ?? [];
        RenumberPages();
    }

    public Guid Id { get; }

    public string? Name { get; private set; }

    public string ScannerId { get; }

    public ScanSettings Settings { get; private set; }

    public ScanDocumentStatus Status { get; private set; }

    public IReadOnlyList<ScannedPage> Pages => _pages;

    public DateTime CreatedAtUtc { get; }

    public DateTime? ClosedAtUtc { get; private set; }

    public static ScanDocument CreateNew(string scannerId, ScanSettings settings, DateTime createdAtUtc, string? name = null) =>
        new(Guid.NewGuid(), scannerId, settings, createdAtUtc, name);

    public void Rename(string? name) =>
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

    public void UpdateSettings(ScanSettings settings)
    {
        EnsureNotClosed();
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public ScannedPage AddPage(string originalFilePath, DateTime scannedAtUtc)
    {
        EnsureNotClosed();

        var page = ScannedPage.CreateNew(_pages.Count + 1, originalFilePath, scannedAtUtc);
        _pages.Add(page);
        return page;
    }

    public void AddPage(ScannedPage page)
    {
        EnsureNotClosed();
        ArgumentNullException.ThrowIfNull(page);

        if (_pages.Any(existing => existing.Id == page.Id))
        {
            throw new DomainException($"Page '{page.Id}' already exists in document '{Id}'.");
        }

        _pages.Add(page);
        RenumberPages();
    }

    public void RemovePage(Guid pageId)
    {
        EnsureNotClosed();

        var removedCount = _pages.RemoveAll(page => page.Id == pageId);
        if (removedCount == 0)
        {
            throw new DomainException($"Page '{pageId}' does not exist in document '{Id}'.");
        }

        RenumberPages();
    }

    public void MovePage(Guid pageId, int newPageNumber)
    {
        EnsureNotClosed();
        Guard.Positive(newPageNumber, nameof(newPageNumber));

        var page = _pages.SingleOrDefault(candidate => candidate.Id == pageId)
            ?? throw new DomainException($"Page '{pageId}' does not exist in document '{Id}'.");

        _pages.Remove(page);
        var targetIndex = Math.Min(newPageNumber - 1, _pages.Count);
        _pages.Insert(targetIndex, page);
        RenumberPages();
    }

    public ScannedPage GetPage(Guid pageId) =>
        _pages.SingleOrDefault(page => page.Id == pageId)
        ?? throw new DomainException($"Page '{pageId}' does not exist in document '{Id}'.");

    public void MarkScanning()
    {
        EnsureNotClosed();
        Status = ScanDocumentStatus.Scanning;
    }

    public void MarkDraft()
    {
        EnsureNotClosed();
        Status = ScanDocumentStatus.Draft;
    }

    public void MarkEditing()
    {
        EnsureNotClosed();
        Status = ScanDocumentStatus.Editing;
    }

    public void MarkExporting()
    {
        EnsureNotClosed();

        if (_pages.Count == 0)
        {
            throw new DomainException("Cannot export a document without pages.");
        }

        Status = ScanDocumentStatus.Exporting;
    }

    public void MarkExported()
    {
        EnsureNotClosed();
        Status = ScanDocumentStatus.Exported;
    }

    public void MarkFailed() =>
        Status = ScanDocumentStatus.Failed;

    public void Close(DateTime closedAtUtc)
    {
        if (Status == ScanDocumentStatus.Closed)
        {
            return;
        }

        ClosedAtUtc = Guard.Utc(closedAtUtc, nameof(closedAtUtc));
        Status = ScanDocumentStatus.Closed;
    }

    private void EnsureNotClosed()
    {
        if (Status == ScanDocumentStatus.Closed)
        {
            throw new DomainException($"Document '{Id}' is closed.");
        }
    }

    private void RenumberPages()
    {
        for (var index = 0; index < _pages.Count; index++)
        {
            _pages[index].SetPageNumber(index + 1);
        }
    }
}
