using DotPulsar;
using DotPulsar.Abstractions;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Text.RegularExpressions;

namespace MQContract.ApachePulsar
{
    internal class Subscription(IPulsarClient pulsarClient, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group) : IServiceSubscription, IAsyncDisposable
    {
        private static readonly Regex regReplyGroup = new Regex(@"^reply-[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$", RegexOptions.Compiled|RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

        private readonly IConsumer<byte[]> consumer = pulsarClient.CreateConsumer<byte[]>(new(group??Guid.NewGuid().ToString(), channel, Schema.ByteArray)
        {
            SubscriptionType = (string.IsNullOrEmpty(group) ? SubscriptionType.Exclusive : SubscriptionType.Shared),
            MessagePrefetchCount = 1,
            InitialPosition = SubscriptionInitialPosition.Latest
        });
        private readonly CancellationTokenSource cancelToken = new();
        private bool disposedValue;

        public void Start()
        {
            if (regReplyGroup.IsMatch(group??string.Empty))
                consumer.Seek(MessageId.Latest);
            _ = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = await consumer.Receive(cancelToken.Token);
                        if (msg!=null)
                            await messageReceived(Connection.ConvertMessage(
                                msg,
                                consumer.Topic,
                                async () => await consumer.Acknowledge(msg.MessageId, cancelToken.Token)
                            )).ConfigureAwait(false);
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
