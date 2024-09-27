using MQContract.Interfaces;
using MQContract.Messages;
using MQContract.Subscriptions;

namespace MQContract
{
    public partial class ContractConnection
    {
        private async ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where T : class
        {
            var messageFactory = GetMessageFactory<T>(ignoreMessageHeader);
            var subscription = new PubSubSubscription<T>(
                async (serviceMessage) =>
                {
                    (var taskMessage, var messageHeader) = await DecodeServiceMessageAsync<T>(ChannelMapper.MapTypes.PublishSubscription, messageFactory, serviceMessage);
                    await messageReceived(new ReceivedMessage<T>(serviceMessage.ID, taskMessage!, messageHeader, serviceMessage.ReceivedTimestamp, DateTime.Now));
                },
                errorReceived,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.PublishSubscription, originalChannel)!,
                channel: channel,
                group: group,
                synchronous: synchronous,
                logger: logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
            => await serviceConnection.PublishAsync(
                await ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish,GetMessageFactory<T>(),message,false,channel,messageHeader),
                cancellationToken
            );

        ValueTask<ISubscription> IContractConnection.SubscribeAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken) where T : class
            => CreateSubscriptionAsync<T>(messageReceived, errorReceived, channel, group, ignoreMessageHeader, false, cancellationToken);

        ValueTask<ISubscription> IContractConnection.SubscribeAsync<T>(Action<IReceivedMessage<T>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken) where T : class
            => CreateSubscriptionAsync<T>((msg) =>
            {
                messageReceived(msg);
                return ValueTask.CompletedTask;
            },
            errorReceived, channel, group, ignoreMessageHeader, true, cancellationToken);
    }
}
