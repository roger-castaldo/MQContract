using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.NATS.Options;
using MQContract.NATS.Subscriptions;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using System.Text;

namespace MQContract.NATS
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using NATS.io
    /// </summary>
    public sealed class Connection : IQueryResponseMessageServiceConnection, IPingableMessageServiceConnection, IAsyncDisposable, IDisposable
    {
        private const string MESSAGE_IDENTIFIER_HEADER = "_MessageID";
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private const string QUERY_RESPONSE_ERROR_TYPE = "NatsQueryError";

        private readonly List<SubscriptionConsumerConfig> subscriptionConsumerConfigs = [];
        private readonly ILogger? logger;
        private bool disposedValue;

        /// <summary>
        /// Houses the underlying NATS service connection being used
        /// </summary>
        public NatsConnection NatsConnection { get; private init; }
        /// <summary>
        /// Houses the underlying JetStream conext being used
        /// </summary>
        public NatsJSContext NatsJSContext { get; private init; }

        /// <summary>
        /// Primary constructor to create an instance using the supplied configuration options.
        /// </summary>
        /// <param name="options"></param>
        public Connection(NatsOpts options)
        {
            NatsConnection = new(options);
            NatsJSContext = new(NatsConnection);
            logger = options.LoggerFactory?.CreateLogger("NatsServiceConnection");
            ProcessConnection().Wait();
        }

        private async Task ProcessConnection()
        {
            await NatsConnection.ConnectAsync();
            if (NatsConnection.ConnectionState == NatsConnectionState.Open)
            {
                if (logger?.IsEnabled(LogLevel.Information)??false)
                {
                    var responseTime = await NatsConnection.PingAsync();
                    logger?.LogInformation("Established connection to [Host:{Address}, Version:{Version}, ResponseTime:{ResponseTime}]",
                        NatsConnection.ServerInfo?.Host,
                        NatsConnection.ServerInfo?.Version,
                        responseTime
                    );
                }
            }
            else
                throw new UnableToConnectException();
        }

        /// <summary>
        /// The maximum message body size allowed.
        /// DEFAULT: 1MB
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 1024*1024*1; //1MB default

        /// <summary>
        /// The default timeout to use for RPC calls when not specified by class or in the call.
        /// DEFAULT: 30 seconds
        /// </summary>
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Called to define a Stream inside the underlying NATS context.  This is an exposure of the NatsJSContext.CreateStreamAsync
        /// </summary>
        /// <param name="streamConfig">The configuration settings for the stream</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The stream creation result</returns>
        public ValueTask<INatsJSStream> CreateStreamAsync(StreamConfig streamConfig, CancellationToken cancellationToken = default)
            => NatsJSContext.CreateStreamAsync(streamConfig, cancellationToken);

        /// <summary>
        /// Called to register a consumer configuration for a given channel.  This is only used for stream channels and allows for configuring
        /// storing and reading patterns
        /// </summary>
        /// <param name="channelName">The underlying stream name that this configuration applies to</param>
        /// <param name="consumerConfig">The consumer configuration to use for that stream</param>
        /// <returns>The underlying connection to allow for chaining</returns>
        public Connection RegisterConsumerConfig(string channelName, ConsumerConfig consumerConfig)
        {
            subscriptionConsumerConfigs.Add(new(channelName, consumerConfig));
            return this;
        }

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
            => new PingResult(NatsConnection.ServerInfo?.Host??string.Empty,
                NatsConnection.ServerInfo?.Version??string.Empty,
                await NatsConnection.PingAsync()
            );

        internal static NatsHeaders ExtractHeader(ServiceMessage message)
            => new(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(
                message.Header.Keys.Select(k =>
                    new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>(
                        k,
                        new Microsoft.Extensions.Primitives.StringValues(message.Header[k]))
                )
                .Concat([
                    new(MESSAGE_IDENTIFIER_HEADER,message.ID),
                    new(MESSAGE_TYPE_HEADER,message.MessageTypeID)
                ])
            ));

        internal static MessageHeader ExtractHeader(NatsHeaders? header, out string? messageID, out string? messageTypeID)
        {
            if (header?.TryGetValue(MESSAGE_IDENTIFIER_HEADER, out var mid)??false)
                messageID = mid.ToString();
            else
                messageID=null;
            if (header?.TryGetValue(MESSAGE_TYPE_HEADER, out var mti)??false)
                messageTypeID=mti.ToString();
            else
                messageTypeID=null;
            return new MessageHeader(header?
                .Where(pair => !Equals(pair.Key, MESSAGE_IDENTIFIER_HEADER)&&!Equals(pair.Key, MESSAGE_TYPE_HEADER))
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString()))?? []
            );
        }

        internal static NatsHeaders ProduceQueryError(Exception exception, string messageID, out byte[] data)
        {
            data = UTF8Encoding.UTF8.GetBytes(exception.Message);
            return new(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>([
                    new KeyValuePair<string,Microsoft.Extensions.Primitives.StringValues>(MESSAGE_IDENTIFIER_HEADER,messageID),
                    new KeyValuePair<string,Microsoft.Extensions.Primitives.StringValues>(MESSAGE_TYPE_HEADER,QUERY_RESPONSE_ERROR_TYPE)
            ]));
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            try
            {
                await NatsConnection.PublishAsync<byte[]>(
                        message.Channel,
                        message.Data.ToArray(),
                        headers: ExtractHeader(message),
                        cancellationToken: cancellationToken
                    );
                return new TransmissionResult(message.ID);
            }
            catch (Exception ex)
            {
                return new TransmissionResult(message.ID, Error: new(ex, ex switch
                {
                    NatsPayloadTooLargeException => true,
                    _ => false
                }));
            }
        }

        async ValueTask<ServiceQueryResult> IQueryResponseMessageServiceConnection.QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                var result = await NatsConnection.RequestAsync<byte[], byte[]>(
                    message.Channel,
                    message.Data.ToArray(),
                    headers: ExtractHeader(message),
                    replyOpts: new() { Timeout = timeout },
                    cancellationToken: cancellationToken
                );
                if (Equals(result.Headers?[MESSAGE_TYPE_HEADER], QUERY_RESPONSE_ERROR_TYPE))
                    throw new TransmissionException(new QueryAsyncReponseException(UTF8Encoding.UTF8.GetString(result.Data!)), true);
                var headers = ExtractHeader(result.Headers, out var messageID, out var messageTypeID);
                return new ServiceQueryResult(
                    messageID??string.Empty,
                    headers,
                    messageTypeID??string.Empty,
                    result.Data??new ReadOnlyMemory<byte>()
                );
            }
            catch (Exception ex)
            {
                throw new TransmissionException(ex, ex switch
                {
                    NatsPayloadTooLargeException => true,
                    ObjectDisposedException => true,
                    ArgumentNullException => true,
                    ArgumentOutOfRangeException => true,
                    _ => false
                });
            }
        }

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            SubscriptionBase subscription;
            var isStream = false;
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
            await foreach (var name in NatsJSContext.ListStreamNamesAsync(cancellationToken: cancellationToken))
            {
                if (Equals(channel, name))
                {
                    isStream=true;
                    break;
                }
            }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
            if (isStream)
            {
                var config = subscriptionConsumerConfigs.Find(scc => Equals(scc.Channel, channel)
                &&(
                    (group==null && string.IsNullOrWhiteSpace(scc.Configuration.Name) && string.IsNullOrWhiteSpace(scc.Configuration.DurableName))
                    ||Equals(group, scc.Configuration.Name)
                    ||Equals(group, scc.Configuration.DurableName)
                ));
                var consumer = await NatsJSContext.CreateOrUpdateConsumerAsync(channel, config?.Configuration??new ConsumerConfig(group??Guid.NewGuid().ToString()) { AckPolicy = ConsumerConfigAckPolicy.Explicit }, cancellationToken);
                subscription = new StreamSubscription(consumer, messageReceived, errorReceived);
            }
            else
                subscription = new PublishSubscription(
                    NatsConnection.SubscribeAsync<byte[]>(
                        channel,
                        queueGroup: group,
                        cancellationToken: cancellationToken
                    ),
                    messageReceived,
                    errorReceived
                );
            subscription.Run();
            return subscription;
        }

        ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var sub = new QuerySubscription(
                NatsConnection.SubscribeAsync<byte[]>(
                    channel,
                    queueGroup: group,
                    cancellationToken: cancellationToken
                ),
                messageReceived,
                errorReceived
            );
            sub.Run();
            return ValueTask.FromResult<IServiceSubscription?>(sub);
        }

        ValueTask IMessageServiceConnection.CloseAsync()
            => NatsConnection.DisposeAsync();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await NatsConnection.DisposeAsync().ConfigureAwait(true);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    NatsConnection.DisposeAsync().AsTask().Wait();
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
