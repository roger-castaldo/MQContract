using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.ApachePulsar
{
    internal class Subscription(IPulsarClient pulsarClient,Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived,string channel,string? group) : IServiceSubscription,IAsyncDisposable
    {
        private readonly IConsumer<byte[]> consumer = pulsarClient.CreateConsumer<byte[]>(new(group??Guid.NewGuid().ToString(), channel, Schema.ByteArray)
        {
            SubscriptionType = (string.IsNullOrEmpty(group) ? SubscriptionType.Exclusive : SubscriptionType.Exclusive),
            MessagePrefetchCount = 1
        });
        private readonly CancellationTokenSource cancelToken = new();
        private bool disposedValue;

        public void Start()
        {
            _ = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = await consumer.Receive(cancelToken.Token);
                        if (msg!=null)
                            messageReceived(Connection.ConvertMessage(
                                msg,
                                consumer.Topic,
                                async () => await consumer.Acknowledge(msg.MessageId,cancelToken.Token)
                            ));
                    }
                    catch (Exception ex)
                    {
                        if (!cancelToken.IsCancellationRequested)
                            errorReceived(ex);
                    }
                }
            });
        }

        async ValueTask IServiceSubscription.EndAsync()
        {
            if (!cancelToken.IsCancellationRequested)
                await cancelToken.CancelAsync();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {

            if (!disposedValue)
            {
                disposedValue=true;
                await ((IServiceSubscription)this).EndAsync();
                await consumer.DisposeAsync();
                cancelToken.Dispose();
            }
        }
    }
}
