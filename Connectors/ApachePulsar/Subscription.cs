using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
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
            SubscriptionType = (string.IsNullOrEmpty(group) || regReplyGroup.IsMatch(group??string.Empty) ? SubscriptionType.Exclusive : SubscriptionType.Shared),
            MessagePrefetchCount = 1,
            InitialPosition = (regReplyGroup.IsMatch(group??string.Empty) ? SubscriptionInitialPosition.Latest : SubscriptionInitialPosition.Earliest)
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
                        {
                            var ackSource = new TaskCompletionSource();
                            await Task.WhenAny(
                                messageReceived(Connection.ConvertMessage(
                                    msg,
                                    consumer.Topic,
                                    async () =>
                                    {
                                        await consumer.Acknowledge(msg.MessageId, cancelToken.Token);
                                        ackSource.SetResult();
                                    }
                                )).AsTask(),
                                ackSource.Task
                            );
                        }
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
            {
                try
                {
                    await consumer.Unsubscribe();
                }
                catch { 
                    //ignore
                }
                await cancelToken.CancelAsync();
            }
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
