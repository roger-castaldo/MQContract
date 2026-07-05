using Azure.Messaging.ServiceBus;
using MQContract.Interfaces.Service;

namespace MQContract.AzureServiceBus
{
    internal class Subscription(ServiceBusClient client, Func<ServiceBusReceivedMessage, Func<Task>, ValueTask> messageRecieved, Action<Exception> errorRecieved, string channel, string? group, string? sessionId = null)
        : IServiceSubscription, IAsyncDisposable
    {
        protected readonly CancellationTokenSource cancelToken = new();
        private ServiceBusReceiver? receiver;
        private bool disposedValue;
        private Task? consumerLoop;

        internal async ValueTask<IServiceSubscription> StartAsync()
        {
            receiver = (sessionId==null ? client.CreateReceiver(channel, group??channel) : await client.AcceptSessionAsync(channel, group??channel, sessionId));
            consumerLoop = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = await receiver.ReceiveMessageAsync(cancellationToken: cancelToken.Token);
                        if (msg!=null)
                        {
                            var ackSource = new TaskCompletionSource();
                            await Task.WhenAny(
                                messageRecieved(msg, async () =>
                                {
                                    await receiver.CompleteMessageAsync(msg);
                                    ackSource.SetResult();
                                }).AsTask(),
                                ackSource.Task
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        errorRecieved(ex);
                    }
                }
            });
            return this;
        }

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
                        await consumerLoop!.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) {
                        // Ignore cancellation exceptions
                    }
                }
                await receiver!.CloseAsync();
                await receiver!.DisposeAsync();
            }
        }

        ValueTask IServiceSubscription.EndAsync()
            => ((IAsyncDisposable)this).DisposeAsync();
    }
}
