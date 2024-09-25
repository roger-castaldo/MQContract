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
    public class Connection : IQueryableMessageServiceConnection,IAsyncDisposable,IDisposable
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase database;
        private readonly Guid connectionID = Guid.NewGuid();
        private bool disposedValue;

        /// <summary>
        /// Default constructor that requires the Redis Configuration settings to be provided
        /// </summary>
        /// <param name="configuration">The configuration to use for the redis connections</param>
        public Connection(ConfigurationOptions configuration)
        {
            connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);
            database = connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Called to define a consumer group inside redis for a given channel
        /// </summary>
        /// <param name="channel">The name of the channel to use</param>
        /// <param name="group">The name of the group to use</param>
        /// <returns>A ValueTask while the operation executes asynchronously</returns>
        public async ValueTask DefineConsumerGroupAsync(string channel,string group)
        {
            if (!(await database.KeyExistsAsync(channel)) ||
                    !(await database.StreamGroupInfoAsync(channel)).Any(x => Equals(x.Name,group)))
            {
                await database.StreamCreateConsumerGroupAsync(channel, group, "0-0", true);
            }
        }

        /// <summary>
        /// The maximum message body size allowed, defaults to 4MB
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 1024*1024*4;

        /// <summary>
        /// The default timeout to allow for a Query Response call to execute, defaults to 1 minute
        /// </summary>
        public TimeSpan DefaultTimout { get; init; } = TimeSpan.FromMinutes(1);

        async ValueTask IMessageServiceConnection.CloseAsync()
            => await connectionMultiplexer.CloseAsync();

        private const string MESSAGE_TYPE_KEY = "_MessageTypeID";
        private const string MESSAGE_ID_KEY = "_MessageID";
        private const string MESSAGE_DATA_KEY = "_MessageData";
        private const string MESSAGE_REPLY_KEY = "_MessageReplyChannel";
        private const string MESSAGE_TIMEOUT_KEY = "_MessageTimeout";

        internal static NameValueEntry[] ConvertMessage(ServiceMessage message, string? replyChannel=null,TimeSpan? messageTimeout=null)
            => message.Header.Keys.Select(k => new NameValueEntry(k, message.Header[k]))
            .Concat(
            [
                new NameValueEntry(MESSAGE_ID_KEY,message.ID),
                new NameValueEntry(MESSAGE_TYPE_KEY,message.MessageTypeID),
                new NameValueEntry(MESSAGE_DATA_KEY,message.Data.ToArray())
            ])
            .Concat(replyChannel==null ? [] : [new NameValueEntry(MESSAGE_REPLY_KEY,replyChannel)])
            .Concat(messageTimeout==null ? [] : [new NameValueEntry(MESSAGE_TIMEOUT_KEY,messageTimeout.ToString())] )
            .ToArray();

        internal static (ReceivedServiceMessage receivedMessage,string? replyChannel,TimeSpan? messageTimeout) ConvertMessage(NameValueEntry[] data, string channel,Func<ValueTask>? acknowledge)
#pragma warning disable S6580 // Use a format provider when parsing date and time
            => (
                new(
                    data.First(nve=>Equals(nve.Name,MESSAGE_ID_KEY)).Value.ToString(),
                    data.First(nve => Equals(nve.Name, MESSAGE_TYPE_KEY)).Value.ToString(),
                    channel,
                    new(data.Where(nve=>!Equals(nve.Name,MESSAGE_ID_KEY)
                        && !Equals(nve.Name,MESSAGE_TYPE_KEY)
                        && !Equals(nve.Name,MESSAGE_DATA_KEY)
                        && !Equals(nve.Name,MESSAGE_REPLY_KEY)
                        && !Equals(nve.Name,MESSAGE_TIMEOUT_KEY)
                    )
                    .Select(nve=>new KeyValuePair<string,string>(nve.Name!,nve.Value.ToString()))),
                    (byte[])data.First(nve=>Equals(nve.Name,MESSAGE_DATA_KEY)).Value!,
                    acknowledge
                ),
                Array.Find(data,(nve)=>Equals(nve.Name,MESSAGE_REPLY_KEY)).Value.ToString(),
                (Array.Exists(data,nve=>Equals(nve.Name,MESSAGE_TIMEOUT_KEY)) ? 
                    TimeSpan.Parse(Array.Find(data, (nve) => Equals(nve.Name, MESSAGE_TIMEOUT_KEY)).Value.ToString())
                    : null)
            );
#pragma warning restore S6580 // Use a format provider when parsing date and time

        internal static string EncodeMessage(ServiceMessage result)
            => JsonSerializer.Serialize<IEnumerable<KeyValuePair<string, object>>>(
                result.Header.Keys.Select(k=> new KeyValuePair<string, object>(k, result.Header[k]!))
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
                data.First(pair=>Equals(pair.Key,MESSAGE_ID_KEY)).Value.GetValue<string>(),
                new(data.Where(pair=>!Equals(pair.Key,MESSAGE_ID_KEY) && !Equals(pair.Key,MESSAGE_TYPE_KEY) && !Equals(pair.Key,MESSAGE_DATA_KEY))
                    .Select(pair=>new KeyValuePair<string,string>(pair.Key,pair.Value.GetValue<string>()))
                ),
                data.First(pair => Equals(pair.Key, MESSAGE_TYPE_KEY)).Value.GetValue<string>(),
                Convert.FromBase64String(data.First(pair => Equals(pair.Key, MESSAGE_DATA_KEY)).Value.GetValue<string>())
            );
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            try
            {
                _ = await database.StreamAddAsync(message.Channel, ConvertMessage(message));
                return new(message.ID);
            }catch(Exception e)
            {
                return new(message.ID, e.Message);
            }
        }

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            if (group!=null)
                await DefineConsumerGroupAsync(channel, group!);
            var result = new PubSubscription(messageReceived, errorReceived, database, connectionID, channel, group);
            await result.StartAsync();
            return result;
        }

        async ValueTask<ServiceQueryResult> IQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var replyID = $"_inbox.{Guid.NewGuid()}";
            await database.StreamAddAsync(message.Channel, ConvertMessage(message, replyID, timeout));
            using var cancellation = new CancellationTokenSource(timeout);
            using var cleanupEntry = cancellationToken.Register(() => cancellation.Cancel());
            while (!cancellation.IsCancellationRequested)
            {
                var keyValue = await database.StringGetDeleteAsync(replyID);
                if (!keyValue.IsNull)
                    return  DecodeMessage(keyValue.ToString());
                else
                    await Task.Delay((int)timeout.TotalMilliseconds/500,cancellationToken);
            }
            throw new TimeoutException();
        }

        async ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            if (group!=null)
                await DefineConsumerGroupAsync(channel, group!);
            var result = new QueryResponseSubscription(messageReceived, errorReceived, database, connectionID, channel, group);
            await result.StartAsync();
            return result;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await connectionMultiplexer.DisposeAsync();

            Dispose(false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    connectionMultiplexer.Dispose();
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
