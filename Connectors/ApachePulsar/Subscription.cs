using DotPulsar;
using DotPulsar.Abstractions;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Text.RegularExpressions;

namespace MQContract.ApachePulsar;

internal class Subscription(IPulsarClient pulsarClient, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group) 
    : BaseLoopSubscription(errorReceived)
{
    private static readonly Regex regReplyGroup = new Regex(@"^reply-[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$", RegexOptions.Compiled|RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

    private readonly IConsumer<byte[]> consumer = pulsarClient.CreateConsumer<byte[]>(new(group??Guid.NewGuid().ToString(), channel, Schema.ByteArray)
    {
        SubscriptionType = (string.IsNullOrEmpty(group) || regReplyGroup.IsMatch(group??string.Empty) ? SubscriptionType.Exclusive : SubscriptionType.Shared),
        MessagePrefetchCount = 1,
        InitialPosition = (regReplyGroup.IsMatch(group??string.Empty) ? SubscriptionInitialPosition.Latest : SubscriptionInitialPosition.Earliest)
    });

    protected override async ValueTask PreStartAsync()
        => await AdjustConsumerForRepliesAsync();

    protected override async ValueTask RecieveMessageAsync(CancellationToken cancelToken)
    {
        var msg = await consumer.Receive(cancelToken);
        if (msg!=null)
        {
            var ackSource = new TaskCompletionSource();
            await Task.WhenAny(
                messageReceived(Connection.ConvertMessage(
                    msg,
                    consumer.Topic,
                    async () =>
                    {
                        await consumer.Acknowledge(msg.MessageId, cancelToken);
                        ackSource.SetResult();
                    }
                )).AsTask(),
                ackSource.Task
            );
        }
    }

    private async Task AdjustConsumerForRepliesAsync()
    {
        if (regReplyGroup.IsMatch(group??string.Empty))
        {
            var ids = await consumer.GetLastMessageIds(cancellationToken: cancelToken.Token);
            if (ids.Any())
                await consumer.Seek(ids.Last(), cancellationToken: cancelToken.Token);
        }
    }

    protected override async ValueTask CleanupAsync()
    {
        try
        {
            await consumer.Unsubscribe();
        }
        catch
        {
            //ignore
        }
        await consumer.DisposeAsync();
    }
}
