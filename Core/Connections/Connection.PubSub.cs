using MQContract.Interfaces;
using MQContract.Messages;

namespace MQContract.Connections
{
    internal partial class Connection
    {
        protected override ValueTask<ISubscription> CreateSubscriptionAsync<T>(Func<IReceivedMessage<T>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, CancellationToken cancellationToken)
            where T : class
            => CreateSubscriptionAsync<T>(
                GetMessageFactory<T>(serviceConnection.MaxMessageBodySize, ignoreMessageHeader), 
                serviceConnection, 
                messageReceived, 
                errorReceived, 
                channel, 
                group, 
                synchronous, 
                cancellationToken
            );

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

        async ValueTask<IEnumerable<TransmissionResult>> IContractConnection.BulkPublishAsync<T>(IEnumerable<(T message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            var serviceMessages = await
                messages.WhenAll(m =>
                    ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes.Publish, GetMessageFactory<T>(serviceConnection.MaxMessageBodySize), m.message, false, channel, m.messageHeader)
                );
            await publishLock.WaitAsync(cancellationToken);
            var result = await BulkPublishAsync(serviceMessages, serviceConnection, cancellationToken);
            publishLock.Release();
            return result;
        }
    }
}
