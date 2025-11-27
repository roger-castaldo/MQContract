using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Collections.Concurrent;

namespace MQContract.InMemory
{
    internal class MessageChannel
    {
        private readonly ConcurrentDictionary<string, MessageGroup> groups = [];

        private async ValueTask<bool> Publish(InternalServiceMessage message, CancellationToken cancellationToken)
        {
            var results = await groups.Values.ToArray().WhenAll(grp => grp.PublishMessageAsync(message, cancellationToken));
            return Array.TrueForAll(results.ToArray(), t => t) && results.Any();
        }

        public void Close()
        {
            var groupsToClose = groups.Values.ToArray();
            groups.Clear();
            foreach (var grp in groupsToClose)
                grp.Close();
        }

        internal async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            if (!await Publish(new(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data), cancellationToken))
                return new(message.ID, Error: new(new TransmissionResultException(), true));
            return new(message.ID);
        }

        internal async ValueTask PublishAsync(InternalServiceMessage message, CancellationToken cancellationToken)
        => await Publish(message, cancellationToken);

        internal async ValueTask<IEnumerable<TransmissionResult>> BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            var messageIDs = messages.Select(m => m.ID).ToArray();
            var grps = groups.Values.ToArray();
            var results = (await messages
                .WhenAll(async (message) =>
                {
                    try
                    {
                        var messageResults = (
                        await grps
                            .WhenAll(grp => grp.PublishMessageAsync(new(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data), cancellationToken))
                        ).ToArray();
                        return new TransmissionResult(message.ID, Error: Array.TrueForAll(messageResults, mr => mr) ? null : new(new TransmissionResultException(), true));
                    }
                    catch
                    {
                        return new TransmissionResult(message.ID, Error: new(new TransmissionResultException(), true));
                    }
                })
                ).OrderBy(res=>Array.IndexOf(messageIDs,res.ID))
                .ToArray();
            return results;
        }

        internal async ValueTask<TransmissionResult> QueryAsync(ServiceMessage message, string inbox, Guid correlationID, CancellationToken cancellationToken)
        {
            if (!await Publish(new(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data, correlationID, inbox), cancellationToken))
                return new(message.ID, new(new TransmissionResultException(), true));
            return new(message.ID);
        }

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
