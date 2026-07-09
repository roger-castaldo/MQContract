using MQContract.Interfaces.Service;

namespace MQContract;

internal abstract class BaseLoopSubscription(Action<Exception> errorReceived) : IServiceSubscription, IAsyncDisposable
{
    private bool disposedValue;
    protected readonly CancellationTokenSource cancelToken = new();
    private Task? consumerLoop;

    protected virtual ValueTask CleanupAsync()
        => ValueTask.CompletedTask;
    protected abstract ValueTask RecieveMessageAsync(CancellationToken cancelToken);
    protected virtual void PreStart()
    {}
    protected virtual ValueTask PreStartAsync()
        => ValueTask.CompletedTask;

    public void Start()
    {
        PreStart();
        consumerLoop = Task.Run(async () =>
        {
            await PreStartAsync();
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    await RecieveMessageAsync(cancelToken.Token);
                }
                catch (OperationCanceledException) when (cancelToken.IsCancellationRequested)
                {
                    //dropped this exception as it can occur when the consumption is stopped
                }
                catch (Exception ex)
                {
                    errorReceived(ex);
                }
            }
        });
    }

    ValueTask IServiceSubscription.EndAsync()
        => ((IAsyncDisposable)this).DisposeAsync();

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (!disposedValue)
        {
            disposedValue=true;
            if (!cancelToken.IsCancellationRequested)
            {
                await cancelToken.CancelAsync();
                try
                {
                    await(consumerLoop??Task.CompletedTask).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // dropped this exception as it can occur when the consumption is stopped
                }
            }
            await CleanupAsync().ConfigureAwait(false);
            cancelToken.Dispose();
        }
    }
}
