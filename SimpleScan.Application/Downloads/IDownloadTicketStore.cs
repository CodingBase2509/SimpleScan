using SimpleScan.Domain.Downloads;

namespace SimpleScan.Application.Downloads;

public interface IDownloadTicketStore
{
    Task SaveAsync(DownloadTicket ticket, CancellationToken cancellationToken);

    Task<DownloadTicket?> FindAsync(string token, CancellationToken cancellationToken);

    Task DeleteAsync(string token, CancellationToken cancellationToken);

    Task DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken);
}
