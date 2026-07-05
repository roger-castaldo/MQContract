using Apache.NMS;
using MQContract.Interfaces.Service;

namespace MQContract.ActiveMQ.Subscriptions
{
    internal class SubscriptionBase(Func<IMessage, TaskCompletionSource, ValueTask> messageReceived, Action<Exception> errorReceived, ConsumerInstance consumer) : IServiceSubscription
    {
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();
        private Task? consumerLoop;

        internal void Start()
        {
            consumerLoop = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = await consumer.ReceiveAsync().WaitAsync(cancelToken.Token);
                        if (msg!=null)
                        {
                            var ackSource = new TaskCompletionSource();
                            await Task.WhenAll(
                                messageReceived(msg, ackSource).AsTask(),
                                ackSource.Task
                            );
                        }
                    }
                    catch (OperationCanceledException)
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

        public ValueTask EndAsync()
            => DisposeAsync();

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                if (!cancelToken.IsCancellationRequested)
                {
                    await cancelToken.CancelAsync();
                    try
                    {
                        await (consumerLoop??Task.CompletedTask).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) {
                        // dropped this exception as it can occur when the consumption is stopped
                    }
                }
                await (consumer?.CloseAsync() ?? Task.CompletedTask).ConfigureAwait(false);
                consumer?.Dispose();
            }
        }
    }
}
