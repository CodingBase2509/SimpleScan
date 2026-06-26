namespace SimpleScan.Application.Events;

public interface IEventPublisher
{
    Task PublishAsync(AppEvent ev, CancellationToken ct);
}