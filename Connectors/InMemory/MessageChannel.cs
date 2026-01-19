using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Collections.Concurrent;

namespace MQContract.InMemory
{
    internal class MessageChannel
    {
        private readonly ConcurrentDictionary<string, MessageGroup> groups = [];

        private async ValueTask<IEnumerable<TransmissionResult>> Publish(IEnumerable<InternalServiceMessage> messages, CancellationToken cancellationToken)
        {
            var results = await groups.Values.ToArray().WhenAll(grp => grp.PublishMessagesAsync(messages, cancellationToken));
            return messages.Select((msg, idx) => new TransmissionResult(msg.ID, Error: Array.TrueForAll(results.Select(r => r.ElementAt(idx)).ToArray(), t => t) ? null : new(new TransmissionResultException(), true)));
        }

        public void Close()
        {
            var groupsToClose = groups.Values.ToArray();
            groups.Clear();
            foreach (var grp in groupsToClose)
                grp.Close();
        }

        internal async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => (await Publish([new(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data)], cancellationToken)).First();

        internal async ValueTask PublishAsync(InternalServiceMessage message, CancellationToken cancellationToken)
            => (await Publish([message], cancellationToken)).First();

        internal async ValueTask<IEnumerable<TransmissionResult>> BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => await Publish(messages.Select(m => new InternalServiceMessage(m.ID, m.MessageTypeID, m.Channel, m.Header, m.Data)), cancellationToken);

        internal async ValueTask<TransmissionResult> QueryAsync(ServiceMessage message, string inbox, Guid correlationID, CancellationToken cancellationToken)
            => (await Publish([new(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data, correlationID, inbox)], cancellationToken)).First();

        private MessageGroup GetGroup(string? group)
        {
            group??=Guid.NewGuid().ToString();
            if (!groups.TryGetValue(group, out MessageGroup? grp))
            {
                grp = new MessageGroup(() =>
                {
                    groups.TryRemove(group,out _);
                });
                groups.TryAdd(group, grp);
            }
            return grp;
        }

        private ValueTask<IServiceSubscription> CreateSubscription(Func<InternalServiceMessage, ValueTask> processMessage, Action<Exception> errorReceived, string? group, CancellationToken cancellationToken)
        {
            var sub = new Subscription(GetGroup(group), async (recievedMessage) =>
            {
                try
                {
                    await processMessage(recievedMessage);
                }
                catch (Exception ex)
                {
                    errorReceived(ex);
                }
            });
            sub.Start();
            return ValueTask.FromResult<IServiceSubscription>(sub);
        }

        internal async ValueTask<IServiceSubscription?> RegisterQuerySubscriptionAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>> messageReceived, Action<Exception> errorReceived, Action<InternalServiceMessage> publishResponse, string? group, CancellationToken cancellationToken)
            => await CreateSubscription(
                async (recievedMessage) =>
                {
                    var result = await messageReceived(new(recievedMessage.ID, recievedMessage.MessageTypeID, recievedMessage.Channel, recievedMessage.Header, recievedMessage.Data));
                    if (result!=null)
                        publishResponse(new(result!.ID, result.MessageTypeID, recievedMessage.ReplyChannel!, result.Header, result.Data, recievedMessage.CorrelationID));
                },
                errorReceived,
                group,
                cancellationToken
            );

        internal async ValueTask<IServiceSubscription?> RegisterSubscriptionAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string? group, CancellationToken cancellationToken)
            => await CreateSubscription(
                (receivedMessage) => messageReceived(new(receivedMessage.ID, receivedMessage.MessageTypeID, receivedMessage.Channel, receivedMessage.Header, receivedMessage.Data)),
                errorReceived,
                group,
                cancellationToken
            );

        internal async ValueTask<IServiceSubscription> EstablishInboxSubscriptionAsync(Func<ReceivedInboxServiceMessage,ValueTask> messageReceived, CancellationToken cancellationToken)
            => await CreateSubscription(
                async (receivedMessage) =>
                {
                    if (receivedMessage.CorrelationID!=null)
                        await messageReceived(new(receivedMessage.ID, receivedMessage.MessageTypeID, receivedMessage.Channel, receivedMessage.Header, receivedMessage.CorrelationID.Value, receivedMessage.Data));
                },
                (error) => { },
                null,
                cancellationToken
            );
    }
}
