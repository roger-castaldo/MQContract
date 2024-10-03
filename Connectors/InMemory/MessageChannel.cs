using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.InMemory
{
    internal class MessageChannel
    {
        private readonly SemaphoreSlim semLock = new(1, 1);
        private readonly List<MessageGroup> groups = [];

        private async ValueTask<bool> Publish(InternalServiceMessage message, CancellationToken cancellationToken)
        {
            await semLock.WaitAsync(cancellationToken);
            var tasks = groups.Select(grp => grp.PublishMessage(message).AsTask()).ToArray();
            semLock.Release();
            await Task.WhenAll(tasks);
            return Array.TrueForAll(tasks, t => t.Result);
        }

        public void Close()
        {
            semLock.Wait();
            foreach (var group in groups.ToArray())
                group.Close();
            groups.Clear();
            semLock.Release();
        }

        internal async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            if (!await Publish(new(message.ID,message.MessageTypeID,message.Channel,message.Header,message.Data), cancellationToken))
                return new(message.ID, "Unable to trasmit");
            return new(message.ID);
        }

        internal async ValueTask PublishAsync(InternalServiceMessage message, CancellationToken cancellationToken)
        => await Publish(message, cancellationToken);

        internal async ValueTask<TransmissionResult> QueryAsync(ServiceMessage message,string inbox, Guid correlationID, CancellationToken cancellationToken)
        {
            if (!await Publish(new(message.ID,message.MessageTypeID,message.Channel,message.Header,message.Data,correlationID,inbox), cancellationToken))
                return new(message.ID, "Unable to trasmit");
            return new(message.ID);
        }

        private async ValueTask<MessageGroup> GetGroupAsync(string? group)
        {
            group??=Guid.NewGuid().ToString();
            await semLock.WaitAsync();
            var grp = groups.Find(g => Equals(g.Group, group));
            if (grp==null)
            {
                grp = new MessageGroup(group, g =>
                {
                    semLock.Wait();
                    groups.Remove(g);
                    semLock.Release();
                });
                groups.Add(grp);
            }
            semLock.Release();
            return grp;
        }

        private async ValueTask<IServiceSubscription> CreateSubscription(Func<InternalServiceMessage,ValueTask>  processMessage,Action<Exception> errorReceived,string? group,CancellationToken cancellationToken)
        {
            var sub = new Subscription(await GetGroupAsync(group), async (recievedMessage) =>
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
            return sub;
        }

        internal async ValueTask<IServiceSubscription?> RegisterQuerySubscriptionAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, Action<InternalServiceMessage> publishResponse, string? group, CancellationToken cancellationToken)
            => await CreateSubscription(
                async (recievedMessage) =>
                {
                    var result = await messageReceived(new(recievedMessage.ID, recievedMessage.MessageTypeID, recievedMessage.Channel, recievedMessage.Header, recievedMessage.Data));
                    publishResponse(new(result.ID, result.MessageTypeID, recievedMessage.ReplyChannel!, result.Header, result.Data, recievedMessage.CorrelationID));
                },
                errorReceived,
                group,
                cancellationToken
            );

        internal async ValueTask<IServiceSubscription?> RegisterSubscriptionAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string? group, CancellationToken cancellationToken)
            => await CreateSubscription(
                (receivedMessage) =>
                {
                    messageReceived(new(receivedMessage.ID, receivedMessage.MessageTypeID, receivedMessage.Channel, receivedMessage.Header, receivedMessage.Data));
                    return ValueTask.CompletedTask;
                },
                errorReceived,
                group,
                cancellationToken
            );

        internal async ValueTask<IServiceSubscription> EstablishInboxSubscriptionAsync(Action<ReceivedInboxServiceMessage> messageReceived, CancellationToken cancellationToken)
            => await CreateSubscription(
                (receivedMessage) =>
                {
                    if (receivedMessage.CorrelationID!=null)
                        messageReceived(new(receivedMessage.ID, receivedMessage.MessageTypeID, receivedMessage.Channel, receivedMessage.Header, receivedMessage.CorrelationID.Value, receivedMessage.Data));
                    return ValueTask.CompletedTask;
                },
                (error) => { },
                null,
                cancellationToken
            );
    }
}
