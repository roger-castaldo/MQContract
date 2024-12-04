using Azure.Messaging.ServiceBus;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Threading.Channels;

namespace MQContract.AzureServiceBus
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using AzureServiceBus
    /// </summary>
    /// <param name="client">The ServiceBusClient to use with this instance</param>
    public sealed class Connection(ServiceBusClient client) : IInboxQueryableMessageServiceConnection,IBulkPublishableMessageServiceConnection, IDisposable
    {
        private const string INBOX_CHANNEL_NAME = "QueryResponse.Inbox";
        private readonly SemaphoreSlim locker = new(1, 1);
        private readonly Guid InboxSessionID = Guid.NewGuid();
        private readonly ServiceBusSender inboxSender = client.CreateSender(INBOX_CHANNEL_NAME);
        private bool disposedValue;

        /// <summary>
        /// Maximum supported message body size in bytes
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 1024*1024; //default 1MB

        TimeSpan IQueryableMessageServiceConnection.DefaultTimeout => TimeSpan.FromMinutes(1);

        async ValueTask IMessageServiceConnection.CloseAsync()
        {
            await locker.WaitAsync();
            await client.DisposeAsync();
            locker.Release();
            locker.Dispose();
        }

        private static ServiceBusMessage ConvertMessage(ServiceMessage message)
        {
            var result = new ServiceBusMessage(message.Data);
            foreach(var k in message.Header.Keys)
            {
                if (message.Header[k]!=null)
                    result.ApplicationProperties.Add(k,message.Header[k]);
            }
            result.MessageId = message.ID;
            result.Subject=message.MessageTypeID;
            return result;
        }

        private static ReceivedServiceMessage ConvertMessage(ServiceBusReceivedMessage message, string channel, Func<Task> acknowledge)
        {
            var headers = new Dictionary<string, string?>();
            foreach (var key in message.ApplicationProperties.Keys)
                headers.Add(key,(string?)message.ApplicationProperties[key]);
            return new(
                message.MessageId,
                message.Subject,
                channel,
                new MessageHeader(headers),
                message.Body.ToArray(),
                async ()=>await acknowledge()
            );
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            await using var sender = client.CreateSender(message.Channel);
            try
            {
                await sender.SendMessageAsync(ConvertMessage(message), cancellationToken);
            }catch(Exception e)
            {
                return new(message.ID, e.Message);
            }
            return new(message.ID);
        }

        async ValueTask<IEnumerable<TransmissionResult>> IBulkPublishableMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
        {
            await using var sender = client.CreateSender(messages.First().Channel);
            using var messageBatch = await sender.CreateMessageBatchAsync();
            foreach(var message in messages)
            {
                if (!messageBatch.TryAddMessage(ConvertMessage(message)))
                    throw new Exception($"The bulk messages are too large for a batch.");
            }
            try
            {
                await sender.SendMessagesAsync(messageBatch);
            }catch(Exception e)
            {
                return messages.Select(m => new TransmissionResult(m.ID, e.Message));
            }
            return messages.Select(m => new TransmissionResult(m.ID));
        }

        private async ValueTask<IServiceSubscription> StartServiceSubscriptionAsync(Subscription subscription)
            => await subscription.StartAsync();

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => await StartServiceSubscriptionAsync(new Subscription(
                client,
                (msg,acknowledge) => {
                    messageReceived(ConvertMessage(msg, channel, acknowledge));
                    return ValueTask.CompletedTask;
                },
                (error) => errorReceived(error),
                channel,
                group
            ));

        async ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Action<ReceivedInboxServiceMessage> messageReceived, CancellationToken cancellationToken)
            => await StartServiceSubscriptionAsync(new Subscription(
                client,
                (msg, acknowledge) => {
                    var result = ConvertMessage(msg,INBOX_CHANNEL_NAME,acknowledge);
                    messageReceived(new ReceivedInboxServiceMessage(
                        result.ID,
                        result.MessageTypeID,
                        INBOX_CHANNEL_NAME,
                        result.Header,
                        new Guid(msg.ReplyToSessionId),
                        result.Data,
                        result.Acknowledge
                    ));
                    return ValueTask.CompletedTask;
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
                return new(message.ID, e.Message);
            }
            return new(message.ID);
        }

        async ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => await StartServiceSubscriptionAsync(new Subscription(
                client,
                async (msg, acknowledge) => {
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

                    var responseMessage = ConvertMessage(response);
                    responseMessage.SessionId = msg.ReplyTo;
                    responseMessage.ReplyToSessionId = msg.ReplyToSessionId;
                    await locker.WaitAsync();
                    try
                    {
                        await inboxSender.SendMessageAsync(responseMessage, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        errorReceived(e);
                    }
                    locker.Release();
                },
                (error) => errorReceived(error),
                channel,
                group
            ));

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    locker.Wait();
                    inboxSender.DisposeAsync().AsTask().Wait();
                    client.DisposeAsync().AsTask().Wait();
                    locker.Release();
                    locker.Dispose();
                }
                disposedValue=true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
