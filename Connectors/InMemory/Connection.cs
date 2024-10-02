using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Collections.Concurrent;

namespace MQContract.InMemory
{
    /// <summary>
    /// Used as an in memory connection messaging system where all transmission are done through Channels within the connection.  You must use the same underlying connection.
    /// </summary>
    public class Connection : IInboxQueryableMessageServiceConnection
    {
        private readonly ConcurrentDictionary<string, MessageChannel> channels = [];
        private readonly string inboxChannel = $"_inbox/{Guid.NewGuid()}";
        /// <summary>
        /// Default timeout for a given QueryResponse call
        /// default: 1 minute
        /// </summary>
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Maximum allowed message body size in bytes
        /// default: 4MB
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 1024*1024*4;

        private MessageChannel GetChannel(string channel)
        {
            if (!channels.TryGetValue(channel, out MessageChannel? messageChannel))
            {
                messageChannel = new MessageChannel();
                channels.TryAdd(channel, messageChannel);
            }
            return messageChannel;
        }

        ValueTask IMessageServiceConnection.CloseAsync()
        {
            var keys = channels.Keys.ToArray();
            foreach(var key in keys)
            {
                if (channels.TryRemove(key, out var channel))
                    channel.Close();
            }
            return ValueTask.CompletedTask;
        }

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => GetChannel(message.Channel).PublishAsync(message, cancellationToken);

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => GetChannel(channel).RegisterSubscriptionAsync(messageReceived, errorReceived, group, cancellationToken);

        ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => GetChannel(channel).RegisterQuerySubscriptionAsync(messageReceived, errorReceived,
                async (response) =>await GetChannel(inboxChannel).PublishAsync(response,cancellationToken), group, cancellationToken);

        ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Action<ReceivedInboxServiceMessage> messageReceived, CancellationToken cancellationToken)
            => GetChannel(inboxChannel).EstablishInboxSubscriptionAsync(messageReceived, cancellationToken);
        ValueTask<TransmissionResult> IInboxQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, Guid correlationID, CancellationToken cancellationToken)
            => GetChannel(message.Channel).QueryAsync(message,inboxChannel, correlationID, cancellationToken);
    }
}
