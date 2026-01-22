using Apache.NMS;
using MQContract.Interfaces.Service;

namespace MQContract.ActiveMQ.Subscriptions
{
    internal class SubscriptionBase(Func<IMessage, TaskCompletionSource, ValueTask> messageReceived, Action<Exception> errorReceived, ConsumerInstance consumer) : IServiceSubscription
    {
        private bool disposedValue;
        protected readonly CancellationTokenSource cancelToken = new();

        internal ValueTask StartAsync()
        {
            _=Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = await consumer.ReceiveAsync();
                        if (msg!=null)
                        {
                            var ackSource = new TaskCompletionSource();
                            await Task.WhenAll(
                                messageReceived(msg, ackSource).AsTask(),
                                ackSource.Task
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        errorReceived(ex);
                    }
                }
            });
            return ValueTask.CompletedTask;
        }

        public async ValueTask EndAsync()
        {
            if (!cancelToken.IsCancellationRequested)
                await cancelToken.CancelAsync();
            if (consumer!=null)
                await consumer.CloseAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await EndAsync();
                consumer?.Dispose();
            }
        }
    }
}
