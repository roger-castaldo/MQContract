using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MQContract.InMemory
{
    internal class MessageChannel : IDisposable
    {
        private readonly Channel<InternalServiceMessage> channel = Channel.CreateBounded<InternalServiceMessage>(new BoundedChannelOptions(10)
        {
            SingleReader=true,
            SingleWriter=false,
            FullMode=BoundedChannelFullMode.Wait
        });
        private readonly ConcurrentDictionary<string, MessageGroup> groups = [];
        private readonly CancellationTokenSource cancelToken = new();

        public MessageChannel()
        {
            _ = Task.Run(async () =>
            {
                while (await channel.Reader.WaitToReadAsync(cancelToken.Token))
                {
                    var message = await channel.Reader.ReadAsync(cancelToken.Token);
                    await groups.Values.Select(grp => grp.PublishMessageAsync(message, cancelToken.Token)).WhenAll().ConfigureAwait(false);
                }
            });
        }

        public void Close()
        {
            channel.Writer.TryComplete();
            if (!cancelToken.IsCancellationRequested)
                cancelToken.Cancel();
            groups.Clear();
        }

        private async ValueTask<TransmissionResult> TryPublishAsync(InternalServiceMessage serviceMessage, CancellationToken cancellationToken)
        {
            if (cancelToken.IsCancellationRequested || groups.IsEmpty)
                return new(serviceMessage.ID, Error: new(new TransmissionResultException(), true));
            try
            {
                await channel.Writer.WriteAsync(serviceMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                return new(serviceMessage.ID, Error: new(ex, true));
            }
            return new(serviceMessage.ID);
        }

        internal ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => TryPublishAsync(new InternalServiceMessage(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data), cancellationToken);

        internal async ValueTask PublishAsync(InternalServiceMessage message, CancellationToken cancellationToken)
            => await channel.Writer.WriteAsync(message, cancellationToken);

        internal ValueTask<TransmissionResult> QueryAsync(ServiceMessage message, string inbox, Guid correlationID, CancellationToken cancellationToken)
            => TryPublishAsync(new InternalServiceMessage(message.ID, message.MessageTypeID, message.Channel, message.Header, message.Data, correlationID, inbox), cancellationToken);

        private MessageGroup GetGroup(string? group)
        {
            group??=Guid.NewGuid().ToString();
            if (!groups.TryGetValue(group, out MessageGroup? grp))
            {
                grp = new MessageGroup(() =>
                {
                    groups.TryRemove(group, out _);
                });
                groups.TryAdd(group, grp);
            }
            return grp;
        }

        private ValueTask<IServiceSubscription> CreateSubscription(Func<InternalServiceMessage, ValueTask> processMessage, Action<Exception> errorReceived, string? group)
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

        internal async ValueTask<IServiceSubscription?> RegisterQuerySubscriptionAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>> messageReceived, Action<Exception> errorReceived, Action<InternalServiceMessage> publishResponse, string? group, CancellationToken _)
            => await CreateSubscription(
                async (recievedMessage) =>
                {
                    var result = await messageReceived(new(recievedMessage.ID, recievedMessage.MessageTypeID, recievedMessage.Channel, recievedMessage.Header, recievedMessage.Data));
                    if (result!=null)
                        publishResponse(new(result!.ID, result.MessageTypeID, recievedMessage.ReplyChannel!, result.Header, result.Data, recievedMessage.CorrelationID));
                },
                errorReceived,
                group
            );

        internal async ValueTask<IServiceSubscription?> RegisterSubscriptionAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string? group, CancellationToken _)
            => await CreateSubscription(
                (receivedMessage) => messageReceived(new(receivedMessage.ID, receivedMessage.MessageTypeID, receivedMessage.Channel, receivedMessage.Header, receivedMessage.Data)),
                errorReceived,
                group
            );

        internal async ValueTask<IServiceSubscription> EstablishInboxSubscriptionAsync(Func<ReceivedInboxServiceMessage, ValueTask> messageReceived, CancellationToken _)
            => await CreateSubscription(
                async (receivedMessage) =>
                {
                    if (receivedMessage.CorrelationID!=null)
                        await messageReceived(new(receivedMessage.ID, receivedMessage.MessageTypeID, receivedMessage.Channel, receivedMessage.Header, receivedMessage.CorrelationID.Value, receivedMessage.Data));
                },
                (error) => { },
                null
            );

        public void Dispose()
        {
            ((IDisposable)cancelToken).Dispose();
        }
    }
}