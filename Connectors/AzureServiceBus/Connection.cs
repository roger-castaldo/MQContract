using Azure.Messaging.ServiceBus;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.AzureServiceBus
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using AzureServiceBus
    /// </summary>
    /// <param name="client">The ServiceBusClient to use with this instance</param>
    /// <param name="pingableQueue">A queue to create a receiver against as a form of pinging to ensure connectivity</param>
    /// <remarks>
    /// In order to use the InboxQueryable capabilites that have been built here you should have a QueryResponse.Inbox Topic and subsequent Subscription 
    /// with RequiresSession as true
    /// </remarks>
    public sealed class Connection(ServiceBusClient client, string? pingableQueue = null)
        : IInboxQueryableMessageServiceConnection, IPingableMessageServiceConnection, IAsyncDisposable
    {
        private const string INBOX_CHANNEL_NAME = "QueryResponse.Inbox";
        private readonly Guid InboxSessionID = Guid.NewGuid();
        private readonly ServiceBusSender inboxSender = client.CreateSender(INBOX_CHANNEL_NAME);
        private bool disposedValue;

        /// <summary>
        /// The supplied service bus client, exposed for additional access if required
        /// </summary>
        public ServiceBusClient BusClient => client;

        /// <summary>
        /// Maximum supported message body size in bytes
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 1024*1024; //default 1MB

        TimeSpan IQueryableMessageServiceConnection.DefaultTimeout => TimeSpan.FromMinutes(1);

        async ValueTask IMessageServiceConnection.CloseAsync()
        {
            await client.DisposeAsync();
        }

        private static ServiceBusMessage ConvertMessage(ServiceMessage message)
        {
            var result = new ServiceBusMessage(message.Data);
            message.Header.ForEach(pair=>result.ApplicationProperties.Add(pair.Key, pair.Value));
            result.MessageId = message.ID;
            result.Subject=message.MessageTypeID;
            return result;
        }

        private static ReceivedServiceMessage ConvertMessage(ServiceBusReceivedMessage message, string channel, Func<Task> acknowledge)
        {
            return new(
                message.MessageId,
                message.Subject,
                channel,
                new MessageHeader(
                    message.ApplicationProperties.Keys
                    .Select(key => new KeyValuePair<string, string?>(key, (string?)message.ApplicationProperties[key]))
                ),
                message.Body.ToArray(),
                async () => await acknowledge()
            );
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            await using var sender = client.CreateSender(message.Channel);
            try
            {
                await sender.SendMessageAsync(ConvertMessage(message), cancellationToken);
            }
            catch (Exception e)
            {
                return new(message.ID, Error: new(e));
            }
            return new(message.ID);
        }

        async ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            await using var sender = client.CreateSender(messages.First().Channel);
            using var messageBatch = await sender.CreateMessageBatchAsync(cancellationToken);
            foreach (var message in messages)
            {
                if (!messageBatch.TryAddMessage(ConvertMessage(message)))
                    throw new BulkTooLargeException();
            }
            try
            {
                await sender.SendMessagesAsync(messageBatch, cancellationToken);
            }
            catch (Exception e)
            {
                return messages.Select(m => new TransmissionResult(m.ID, Error: new(e)));
            }
            return messages.Select(m => new TransmissionResult(m.ID));
        }

        private static async ValueTask<IServiceSubscription> StartServiceSubscriptionAsync(Subscription subscription)
            => await subscription.StartAsync();

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => await StartServiceSubscriptionAsync(new Subscription(
                client,
                (msg, acknowledge) => messageReceived(ConvertMessage(msg, channel, acknowledge)),
                (error) => errorReceived(error),
                channel,
                group
            ));

        async ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Func<ReceivedInboxServiceMessage, ValueTask> messageReceived, CancellationToken cancellationToken)
            => await StartServiceSubscriptionAsync(new Subscription(
                client,
                async (msg, acknowledge) =>
                {
                    var result = ConvertMessage(msg, INBOX_CHANNEL_NAME, acknowledge);
                    await messageReceived(new ReceivedInboxServiceMessage(
                        result.ID,
                        result.MessageTypeID,
                        INBOX_CHANNEL_NAME,
                        result.Header,
                        new Guid(msg.ReplyToSessionId),
                        result.Data,
                        result.Acknowledge
                    ));
                },
                (error) => { },
                INBOX_CHANNEL_NAME,
                INBOX_CHANNEL_NAME,
                InboxSessionID.ToString()
            ));

        async ValueTask<TransmissionResult> IInboxQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, Guid correlationID, CancellationToken cancellationToken)
        {
            var azureMessage = ConvertMessage(message);
            azureMessage.ReplyToSessionId = correlationID.ToString();
            azureMessage.ReplyTo = InboxSessionID.ToString();
            await using var sender = client.CreateSender(message.Channel);
            try
            {
                await sender.SendMessageAsync(azureMessage, cancellationToken);
            }
            catch (Exception e)
            {
                return new(message.ID, Error: new(e, e switch
                {
                    ObjectDisposedException => true,
                    OperationCanceledException => true,
                    _ => false
                }));
            }
            return new(message.ID);
        }

        async ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => await StartServiceSubscriptionAsync(new Subscription(
                client,
                async (msg, acknowledge) =>
                {
                    var result = ConvertMessage(msg, INBOX_CHANNEL_NAME, acknowledge);
                    var response = await messageReceived(new ReceivedInboxServiceMessage(
                        result.ID,
                        result.MessageTypeID,
                        INBOX_CHANNEL_NAME,
                        result.Header,
                        new Guid(msg.ReplyToSessionId),
                        result.Data,
                        result.Acknowledge
                    ));
                    if (response!=null)
                    {
                        var responseMessage = ConvertMessage(response!);
                        responseMessage.SessionId = msg.ReplyTo;
                        responseMessage.ReplyToSessionId = msg.ReplyToSessionId;
                        try
                        {
                            await inboxSender.SendMessageAsync(responseMessage, cancellationToken);
                        }
                        catch (Exception e)
                        {
                            errorReceived(e);
                        }
                    }
                },
                (error) => errorReceived(error),
                channel,
                group
            ));

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            var start = Stopwatch.GetTimestamp();
            try
            {
                await using var receiver = client.CreateReceiver(pingableQueue??"pingable");
                _ = await receiver.PeekMessageAsync();
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
                return new(string.Empty, string.Empty, Stopwatch.GetElapsedTime(start));
            }
            catch
            {
                throw new PingFailedException("Unable to create a test receiver to the service bus");
            }
            return new(string.Empty, string.Empty, Stopwatch.GetElapsedTime(start));
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue = true;
                await inboxSender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
