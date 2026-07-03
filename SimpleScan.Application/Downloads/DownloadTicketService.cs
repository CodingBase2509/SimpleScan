using System.Security.Cryptography;

namespace SimpleScan.Application.Downloads;

public sealed class DownloadTicketService
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromHours(1);

    private readonly IScanDocumentStore _documents;
    private readonly IDownloadTicketStore _tickets;
    private readonly TimeProvider _timeProvider;

    public DownloadTicketService(
        IScanDocumentStore documents,
        IDownloadTicketStore tickets,
        TimeProvider timeProvider)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        _tickets = tickets ?? throw new ArgumentNullException(nameof(tickets));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public Task<DownloadTicket> CreatePdfExportTicketAsync(
        Guid documentId,
        CancellationToken cancellationToken) =>
        CreatePdfExportTicketAsync(documentId, "scan.pdf", DefaultLifetime, cancellationToken);

    public async Task<DownloadTicket> CreatePdfExportTicketAsync(
        Guid documentId,
        string fileName,
        TimeSpan lifetime,
        CancellationToken cancellationToken)
    {
        if (lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Lifetime must be greater than zero.");
        }

        var document = await _documents.FindAsync(documentId, cancellationToken)
            ?? throw new DomainException($"Scan document '{documentId}' was not found.");

        if (document.Pages.Count == 0)
        {
            throw new DomainException("Cannot create a download ticket for a document without pages.");
        }

        var createdAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var ticket = new DownloadTicket(
            CreateToken(),
            document.Id,
            filePath: null,
            fileName,
            createdAtUtc,
            createdAtUtc.Add(lifetime));

        await _tickets.SaveAsync(ticket, cancellationToken);
        return ticket;
    }

    private static string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
