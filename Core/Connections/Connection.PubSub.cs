using MQContract.Interfaces.Service;
using MQContract.Interfaces;
using MQContract.Messages;
using MQContract.Subscriptions;

namespace MQContract.Connections
{
    internal partial class Connection
    {
        private async ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where T : class
        {
            var messageFactory = GetMessageFactory<T>(serviceConnection.MaxMessageBodySize,ignoreMessageHeader);
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
                logger: Logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }

        async ValueTask<TransmissionResult> IContractConnection.PublishAsync<T>(T message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            await publishLock.WaitAsync(cancellationToken);
            var result = await serviceConnection.PublishAsync(
                await ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(serviceConnection.MaxMessageBodySize), message, false, channel, messageHeader),
                cancellationToken
            );
            publishLock.Release();
            return result;
        }

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<T>(IEnumerable<(T message, MessageHeader? messageHeader)> messages, string? channel = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var serviceMessages = await Task.WhenAll(
                messages.Select(m =>
                    ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(serviceConnection.MaxMessageBodySize), m.message, false, channel, m.messageHeader).AsTask())
                .ToArray()
            );
            IEnumerable<TransmissionResult> result = [];
            await publishLock.WaitAsync(cancellationToken);
            if (serviceConnection is IBulkPublishableMessageServiceConnection bulkPublishableMessageServiceConnection)
                result = await bulkPublishableMessageServiceConnection.BulkPublishAsync(serviceMessages, cancellationToken);
            else
                foreach (var message in serviceMessages)
                    result=result.Append(await serviceConnection.PublishAsync(message, cancellationToken));
            publishLock.Release();
            return result;
        }

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
