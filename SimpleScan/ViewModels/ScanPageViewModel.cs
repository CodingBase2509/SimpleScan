namespace SimpleScan.ViewModels;

public sealed class ScanPageViewModel
{
    public Guid DocumentId { get; private set; }
    public Guid? SelectedPageId { get; private set; }

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public Task InitializeAsync(Guid documentId, Guid? pageId)
    {
        DocumentId = documentId;
        SelectedPageId = pageId;
        return Task.CompletedTask;
    }
}
