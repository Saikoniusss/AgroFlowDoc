using System.Threading.Channels;

namespace Infrastructure.Services;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);
    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundTaskQueue(int capacity = 500)
    {
        BoundedChannelOptions options = new(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem)
        => await _queue.Writer.WriteAsync(workItem);

    public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        => await _queue.Reader.ReadAsync(cancellationToken);
}