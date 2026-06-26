namespace SimpleScan.Application.Events;

public interface IEventSubscriber
{
    IDisposable Subscribe(string eventKey, Func<AppEvent, CancellationToken, Task> handler);
}