using System.Collections.Concurrent;
using SimpleScan.Application.Events;

namespace SimpleScan.Infrastructure.Events;

public sealed class InMemoryEventBus : IEventPublisher, IEventSubscriber
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Func<AppEvent, CancellationToken, Task>>> _handlers = new();

    public IDisposable Subscribe(string eventKey, Func<AppEvent, CancellationToken, Task> handler)
    {
        if (string.IsNullOrWhiteSpace(eventKey))
        {
            throw new ArgumentException("Event key cannot be empty.", nameof(eventKey));
        }

        ArgumentNullException.ThrowIfNull(handler);

        var subscriptionId = Guid.NewGuid();
        var handlers = _handlers.GetOrAdd(eventKey.Trim(), _ => new ConcurrentDictionary<Guid, Func<AppEvent, CancellationToken, Task>>());
        handlers[subscriptionId] = handler;

        return new Subscription(() =>
        {
            if (_handlers.TryGetValue(eventKey.Trim(), out var currentHandlers))
            {
                currentHandlers.TryRemove(subscriptionId, out _);
            }
        });
    }

    public async Task PublishAsync(AppEvent ev, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ev);
        ct.ThrowIfCancellationRequested();

        if (!_handlers.TryGetValue(ev.Type, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers.Values)
        {
            ct.ThrowIfCancellationRequested();
            await handler(ev, ct);
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private int _disposed;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _unsubscribe();
            }
        }
    }
}
