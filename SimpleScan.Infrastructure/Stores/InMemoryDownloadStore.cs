using System.Collections.Concurrent;
using SimpleScan.Application.Downloads;
using SimpleScan.Domain.Downloads;

namespace SimpleScan.Infrastructure.Stores;

public sealed class InMemoryDownloadStore : IDownloadTicketStore
{
    private readonly ConcurrentDictionary<string, DownloadTicket> _tickets = new();
    
    public Task SaveAsync(DownloadTicket ticket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        cancellationToken.ThrowIfCancellationRequested();

        _tickets[ticket.Token] = ticket;
        return Task.CompletedTask;
    }

    public Task<DownloadTicket?> FindAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<DownloadTicket?>(null);
        }

        return _tickets.TryGetValue(token, out DownloadTicket? ticket)
            ? Task.FromResult(ticket)
            : Task.FromResult<DownloadTicket?>(null);
    }

    public Task DeleteAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(token))
        {
            return Task.CompletedTask;
        }
        
        _tickets.TryRemove(token, out _);
        return Task.CompletedTask;
    }

    public Task DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var keys = _tickets
            .Where(kvp => kvp.Value.IsExpired(utcNow))
            .Select(kvp => kvp.Key);
        
        foreach (var key in keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _tickets.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
