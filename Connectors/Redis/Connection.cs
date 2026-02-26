using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Redis.Subscriptions;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MQContract.Redis
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using Redis
    /// </summary>
    public sealed class Connection : IQueryResponseMessageServiceConnection, IPingableMessageServiceConnection, IAsyncDisposable
    {
        private readonly record struct MessageInstance(string ID, string Channel, NameValueEntry[] Data);

        private readonly Guid connectionID = Guid.NewGuid();
        private readonly BatchedMessageStream<MessageInstance> batchedMessageStream;
        private bool disposedValue;

        /// <summary>
        /// Houses the underlying Connection Multiplexer being used
        /// </summary>
        public ConnectionMultiplexer ConnectionMultiplexer { get; private init; }
        /// <summary>
        /// Houses the underlying Database being used
        /// </summary>
        public IDatabase Database { get; private init; }

        /// <summary>
        /// Default constructor that requires the Redis Configuration settings to be provided
        /// </summary>
        /// <param name="configuration">The configuration to use for the redis connections</param>
        public Connection(ConfigurationOptions configuration)
        {
            ConnectionMultiplexer = ConnectionMultiplexer.Connect(configuration);
            Database = ConnectionMultiplexer.GetDatabase();
            batchedMessageStream = new(
                async (serviceMessage, _) => new MessageInstance(
                    serviceMessage.ID,
                    serviceMessage.Channel,
                    ConvertMessage(serviceMessage)
                ),
                async (messageInstance, cancellationToken) =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return new TransmissionResult(messageInstance.ID, Error: new(new OperationCanceledException("Transmission cancelled"), true));
                        _ = await Database.StreamAddAsync(messageInstance.Channel, messageInstance.Data);
                        return new TransmissionResult(messageInstance.ID);
                    }
                    catch (Exception e)
                    {
                        return new TransmissionResult(messageInstance.ID, Error: new(e));
                    }
                }
            );
        }

        /// <summary>
        /// Called to define a consumer group inside redis for a given channel
        /// </summary>
        /// <param name="channel">The name of the channel to use</param>
        /// <param name="group">The name of the group to use</param>
        /// <returns>A ValueTask while the operation executes asynchronously</returns>
        public async ValueTask DefineConsumerGroupAsync(string channel, string group)
        {
            if (!(await Database.KeyExistsAsync(channel)) ||
                    !(await Database.StreamGroupInfoAsync(channel)).Any(x => Equals(x.Name, group)))
            {
                await Database.StreamCreateConsumerGroupAsync(channel, group, "0-0", true);
            }
        }

        /// <summary>
        /// The maximum message body size allowed, defaults to 4MB
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 1024*1024*4;

        /// <summary>
        /// The default timeout to allow for a Query Response call to execute, defaults to 1 minute
        /// </summary>
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromMinutes(1);

        async ValueTask IMessageServiceConnection.CloseAsync()
        {
            await batchedMessageStream.DisposeAsync();
            await ConnectionMultiplexer.CloseAsync();
        }

        private const string MESSAGE_TYPE_KEY = "_MessageTypeID";
        private const string MESSAGE_ID_KEY = "_MessageID";
        private const string MESSAGE_DATA_KEY = "_MessageData";
        private const string MESSAGE_REPLY_KEY = "_MessageReplyChannel";
        private const string MESSAGE_TIMEOUT_KEY = "_MessageTimeout";

        internal static NameValueEntry[] ConvertMessage(ServiceMessage message, string? replyChannel = null, TimeSpan? messageTimeout = null)
            =>
            [
                new NameValueEntry(MESSAGE_ID_KEY,message.ID),
                new NameValueEntry(MESSAGE_TYPE_KEY,message.MessageTypeID),
                new NameValueEntry(MESSAGE_DATA_KEY,message.Data.ToArray()),
                .. message.Header.Select(pair => new NameValueEntry(pair.Key, pair.Value))
                    .Concat(replyChannel==null ? [] : [new NameValueEntry(MESSAGE_REPLY_KEY, replyChannel)])
                    .Concat(messageTimeout==null ? [] : [new NameValueEntry(MESSAGE_TIMEOUT_KEY, messageTimeout.ToString())])
            ];

        internal static (ReceivedServiceMessage receivedMessage, string? replyChannel, TimeSpan? messageTimeout, TaskCompletionSource ackSource) ConvertMessage(NameValueEntry[] data, string channel, Func<ValueTask>? acknowledge)
#pragma warning disable S6580 // Use a format provider when parsing date and time
        {
            var ackSource = new TaskCompletionSource();
            return (
                new(
                    data.First(nve => Equals(nve.Name, MESSAGE_ID_KEY)).Value.ToString(),
                    data.First(nve => Equals(nve.Name, MESSAGE_TYPE_KEY)).Value.ToString(),
                    channel,
                    new(data.Where(nve => !Equals(nve.Name, MESSAGE_ID_KEY)
                        && !Equals(nve.Name, MESSAGE_TYPE_KEY)
                        && !Equals(nve.Name, MESSAGE_DATA_KEY)
                        && !Equals(nve.Name, MESSAGE_REPLY_KEY)
                        && !Equals(nve.Name, MESSAGE_TIMEOUT_KEY)
                    )
                    .Select(nve => new KeyValuePair<string, string?>(nve.Name!, nve.Value.ToString()))),
                    (byte[])data.First(nve => Equals(nve.Name, MESSAGE_DATA_KEY)).Value!,
                    acknowledge: async() =>
                    {
                        if (acknowledge!=null)
                            await acknowledge();
                        ackSource.TrySetResult();
                    }
                ),
                Array.Find(data, (nve) => Equals(nve.Name, MESSAGE_REPLY_KEY)).Value.ToString(),
                (Array.Exists(data, nve => Equals(nve.Name, MESSAGE_TIMEOUT_KEY)) ?
                    TimeSpan.Parse(Array.Find(data, (nve) => Equals(nve.Name, MESSAGE_TIMEOUT_KEY)).Value.ToString())
                    : null),
                ackSource
            );
        }
#pragma warning restore S6580 // Use a format provider when parsing date and time

        internal static string EncodeMessage(ServiceMessage result)
            => JsonSerializer.Serialize<IEnumerable<KeyValuePair<string, object>>>(
                result.Header.Keys.Select(k => new KeyValuePair<string, object>(k, result.Header[k]!))
                .Concat([
                    new KeyValuePair<string,object>(MESSAGE_ID_KEY, result.ID),
                    new KeyValuePair<string,object>(MESSAGE_TYPE_KEY,result.MessageTypeID),
                    new KeyValuePair<string,object>(MESSAGE_DATA_KEY,result.Data)
                ])
            );

        internal static ServiceQueryResult DecodeMessage(string content)
        {
            var data = JsonSerializer.Deserialize<IEnumerable<KeyValuePair<string, JsonNode>>>(content)!;
            return new(
                data.First(pair => Equals(pair.Key, MESSAGE_ID_KEY)).Value.GetValue<string>(),
                new(data.Where(pair => !Equals(pair.Key, MESSAGE_ID_KEY) && !Equals(pair.Key, MESSAGE_TYPE_KEY) && !Equals(pair.Key, MESSAGE_DATA_KEY))
                    .Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value.GetValue<string>()))
                ),
                data.First(pair => Equals(pair.Key, MESSAGE_TYPE_KEY)).Value.GetValue<string>(),
                Convert.FromBase64String(data.First(pair => Equals(pair.Key, MESSAGE_DATA_KEY)).Value.GetValue<string>())
            );
        }

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(message, cancellationToken);

        ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(messages, cancellationToken);

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            if (group!=null)
                await DefineConsumerGroupAsync(channel, group!);
            var result = new PubSubscription(messageReceived, errorReceived, Database, connectionID, channel, group);
            await result.StartAsync();
            return result;
        }

        async ValueTask<ServiceQueryResult> IQueryResponseMessageServiceConnection.QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var replyID = $"_inbox.{Guid.NewGuid()}";
            await Database.StreamAddAsync(message.Channel, ConvertMessage(message, replyID, timeout));
            using var cancellation = new CancellationTokenSource(timeout);
            using var cleanupEntry = cancellationToken.Register(() => cancellation.Cancel());
            while (!cancellation.IsCancellationRequested)
            {
                var keyValue = await Database.StringGetDeleteAsync(replyID);
                if (!keyValue.IsNull)
                    return DecodeMessage(keyValue.ToString());
                else
                    await Task.Delay((int)timeout.TotalMilliseconds/500, cancellationToken);
            }
            throw new TransmissionException(new TimeoutException());
        }

        async ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            if (group!=null)
                await DefineConsumerGroupAsync(channel, group!);
            var result = new QueryResponseSubscription(messageReceived, errorReceived, Database, connectionID, channel, group);
            await result.StartAsync();
            return result;
        }

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            var server = ConnectionMultiplexer.GetServers().FirstOrDefault(s => s.IsConnected);
            if (server!=null)
                return new(string.Empty, server.Version.ToString(), await server.PingAsync());
            throw new PingFailedException("Unable to find connected server to ping");
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await batchedMessageStream.DisposeAsync().ConfigureAwait(true);
                await ConnectionMultiplexer.DisposeAsync().ConfigureAwait(true);
            }
            GC.SuppressFinalize(this);
        }
    }
}
