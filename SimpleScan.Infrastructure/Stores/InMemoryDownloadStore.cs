using System.Collections.Concurrent;
using SimpleScan.Application.Downloads;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Downloads;

namespace SimpleScan.Infrastructure.Stores;

public class InMemoryDownloadStore : IDownloadTicketStore
{
    private readonly ConcurrentDictionary<string, DownloadTicket> _tickets = new();
    
    public Task SaveAsync(DownloadTicket ticket, CancellationToken cancellationToken)
    {
        if (_tickets.TryAdd(ticket.Token, ticket))
            throw new DomainException($"Ticket with id {ticket.Token} can not be stored");
        return Task.CompletedTask;
    }

    public Task<DownloadTicket?> FindAsync(string token, CancellationToken cancellationToken)
    {
        return _tickets.TryGetValue(token, out DownloadTicket? ticket)
            ? Task.FromResult(ticket)
            : Task.FromResult<DownloadTicket?>(null);
    }

    public Task DeleteAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
            return Task.CompletedTask;
        
        _tickets.TryRemove(token, out _);
        return Task.CompletedTask;
    }

    public Task DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var keys = _tickets.Where(kvp => kvp.Value.ExpiresAtUtc > utcNow)
            .Select(kvp => kvp.Key);
        
        foreach (var key in keys)
            _tickets.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}