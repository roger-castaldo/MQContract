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

        internal async Task<IServiceSubscription> StartAsync()
        {
            receiver = (sessionId==null ? client.CreateReceiver(channel, group??channel) : await client.AcceptSessionAsync(channel, group??channel, sessionId));
            _ = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = await receiver.ReceiveMessageAsync(cancellationToken: cancelToken.Token);
                        if (msg!=null)
                            await messageRecieved(msg, async () => await receiver.CompleteMessageAsync(msg));
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
                await ((IServiceSubscription)this).EndAsync();
                await receiver!.DisposeAsync();
            }
        }

        async ValueTask IServiceSubscription.EndAsync()
        {
            if (!cancelToken.IsCancellationRequested)
                await cancelToken.CancelAsync();
            await receiver!.CloseAsync();
        }
    }
}
