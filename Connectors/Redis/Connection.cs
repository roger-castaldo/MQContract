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
        /// <param name="configuration"></param>
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
        public TimeSpan DefaultTimout => TimeSpan.FromMinutes(1);

        /// <summary>
        /// Called to close off the underlying Redis Connection
        /// </summary>
        /// <returns></returns>
        public async ValueTask CloseAsync()
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

        internal static (RecievedServiceMessage recievedMessage,string? replyChannel,TimeSpan? messageTimeout) ConvertMessage(NameValueEntry[] data, string channel,Func<ValueTask>? acknowledge)
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

        /// <summary>
        /// Called to publish a message into the Redis server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Transmition result identifying if it worked or not</returns>
        public async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Called to create a subscription to the underlying Redis server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a message is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The name of the group to bind the consumer to</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        public async ValueTask<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string? group = null, CancellationToken cancellationToken = default)
        {
            if (group!=null)
                await DefineConsumerGroupAsync(channel, group!);
            var result = new PubSubscription(messageRecieved, errorRecieved, database, connectionID, channel, group);
            await result.StartAsync();
            return result;
        }

        /// <summary>
        /// Called to publish a query into the Redis server 
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="timeout">The timeout supplied for the query to response</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The resulting response</returns>
        /// <exception cref="TimeoutException">Thrown when the response times out</exception>
        public async ValueTask<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Called to create a subscription for queries to the underlying Redis server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a query is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The group to bind to</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance</returns>
        public async ValueTask<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, ValueTask<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string? group = null, CancellationToken cancellationToken = default)
        {
            if (group!=null)
                await DefineConsumerGroupAsync(channel, group!);
            var result = new QueryResponseSubscription(messageRecieved, errorRecieved, database, connectionID, channel, group);
            await result.StartAsync();
            return result;
        }

        /// <summary>
        /// Called to dispose of the object correctly and allow it to clean up it's resources
        /// </summary>
        /// <returns>A task required for disposal</returns>
        public async ValueTask DisposeAsync()
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

        /// <summary>
        /// Called to dispose of the object correctly and allow it to clean up it's resources
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
