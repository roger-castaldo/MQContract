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
    public class Connection : IMessageServiceConnection
    {
        private const string MESSAGE_IDENTIFIER_HEADER = "_MessageID";
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private const string QUERY_RESPONSE_ERROR_TYPE = "NatsQueryError";

        private readonly NatsConnection natsConnection;
        private readonly NatsJSContext natsJSContext;
        private readonly ILogger? logger;
        private bool disposedValue;

        /// <summary>
        /// Primary constructor to create an instance using the supplied configuration options.
        /// </summary>
        /// <param name="options"></param>
        public Connection(NatsOpts options)
        {
            natsConnection = new(options);
            natsJSContext = new(natsConnection);
            logger = options.LoggerFactory?.CreateLogger("NatsServiceConnection");
            ProcessConnection().Wait();
        }

        private async Task ProcessConnection()
        {
            await natsConnection.ConnectAsync();
            if (natsConnection.ConnectionState == NatsConnectionState.Open)
            {
                var responseTime = await natsConnection.PingAsync();
                logger?.LogInformation("Established connection to [Host:{Address}, Version:{Version}, ResponseTime:{ResponseTime}]",
                    natsConnection.ServerInfo?.Host,
                    natsConnection.ServerInfo?.Version,
                    responseTime
                );
            }
            else
                throw new UnableToConnectException();
        }

        /// <summary>
        /// The maximum message body size allowed.
        /// DEFAULT: 1MB
        /// </summary>
        public int? MaxMessageBodySize { get; init; } = 1024*1024*1; //1MB default

        /// <summary>
        /// The default timeout to use for RPC calls when not specified by class or in the call.
        /// DEFAULT: 30 seconds
        /// </summary>
        public TimeSpan DefaultTimout { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Called to define a Stream inside the underlying NATS context.  This is an exposure of the NatsJSContext.CreateStreamAsync
        /// </summary>
        /// <param name="streamConfig">The configuration settings for the stream</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The stream creation result</returns>
        public ValueTask<INatsJSStream> CreateStreamAsync(StreamConfig streamConfig,CancellationToken cancellationToken = default)
            => natsJSContext.CreateStreamAsync(streamConfig, cancellationToken);

        /// <summary>
        /// Called to ping the NATS.io service
        /// </summary>
        /// <returns>The Ping Result including service information</returns>
        public async ValueTask<PingResult> PingAsync()
            => new PingResult(natsConnection.ServerInfo?.Host??string.Empty,
                natsConnection.ServerInfo?.Version??string.Empty,
                await natsConnection.PingAsync()
            );

        internal static NatsHeaders ExtractHeader(ServiceMessage message)
            => new(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(
                message.Header.Keys.Select(k=>
                    new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>(
                        k,
                        new Microsoft.Extensions.Primitives.StringValues(message.Header[k]))
                )
                .Concat([
                    new(MESSAGE_IDENTIFIER_HEADER,message.ID),
                    new(MESSAGE_TYPE_HEADER,message.MessageTypeID)
                ])
            ));

        internal static MessageHeader ExtractHeader(NatsHeaders? header,out string? messageID,out string? messageTypeID)
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
                .Where(pair=>!Equals(pair.Key,MESSAGE_IDENTIFIER_HEADER)&&!Equals(pair.Key,MESSAGE_TYPE_HEADER))
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString()))?? []
            );
        }

        internal static NatsHeaders ProduceQueryError(Exception exception,string messageID,out byte[] data)
        {
            data = UTF8Encoding.UTF8.GetBytes(exception.Message);
            return new(new Dictionary<string,Microsoft.Extensions.Primitives.StringValues>([
                    new KeyValuePair<string,Microsoft.Extensions.Primitives.StringValues>(MESSAGE_IDENTIFIER_HEADER,messageID),
                    new KeyValuePair<string,Microsoft.Extensions.Primitives.StringValues>(MESSAGE_TYPE_HEADER,QUERY_RESPONSE_ERROR_TYPE)
            ]));
        }

        /// <summary>
        /// Called to publish a message into the NATS io server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="options">The service channel options, if desired, specifically the StreamPublishChannelOptions which is used to access streams vs standard publish method</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Transmition result identifying if it worked or not</returns>
        /// <exception cref="InvalidChannelOptionsTypeException">Thrown when an attempt to pass an options object that is not of the type StreamPublishChannelOptions</exception>
        public async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            InvalidChannelOptionsTypeException.ThrowIfNotNullAndNotOfType<StreamPublishChannelOptions>(options);
            try
            {
                if (options is StreamPublishChannelOptions publishChannelOptions)
                {
                    if (publishChannelOptions.Config!=null)
                        await CreateStreamAsync(publishChannelOptions.Config, cancellationToken);
                    var ack = await natsJSContext.PublishAsync<byte[]>(
                        message.Channel,
                        message.Data.ToArray(),
                        headers: ExtractHeader(message),
                        cancellationToken: cancellationToken
                    );
                    return new TransmissionResult(message.ID, (ack.Error!=null ? $"{ack.Error.Code}:{ack.Error.Description}" : null));
                }
                else
                {
                    await natsConnection.PublishAsync<byte[]>(
                        message.Channel,
                        message.Data.ToArray(),
                        headers: ExtractHeader(message),
                        cancellationToken: cancellationToken
                    );
                    return new TransmissionResult(message.ID);
                }
            }catch(Exception ex)
            {
                return new TransmissionResult(message.ID, ex.Message);
            }
        }

        /// <summary>
        /// Called to publish a query into the NATS io server 
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="timeout">The timeout supplied for the query to response</param>
        /// <param name="options">Should be null here as there is no Service Channel Options implemented for this call</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The resulting response</returns>
        /// <exception cref="NoChannelOptionsAvailableException">Thrown if options was supplied because there are no implemented options for this call</exception>
        /// <exception cref="QueryAsyncReponseException">Thrown when an error comes from the responding service</exception>
        public async ValueTask<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            NoChannelOptionsAvailableException.ThrowIfNotNull(options);
            var result = await natsConnection.RequestAsync<byte[], byte[]>(
                message.Channel,
                message.Data.ToArray(),
                headers: ExtractHeader(message),
                replyOpts: new() { Timeout = timeout },
                cancellationToken: cancellationToken
            );
            if (Equals(result.Headers?[MESSAGE_TYPE_HEADER], QUERY_RESPONSE_ERROR_TYPE))
                throw new QueryAsyncReponseException(UTF8Encoding.UTF8.GetString(result.Data!));
            var headers = ExtractHeader(result.Headers, out var messageID, out var messageTypeID);
            return new ServiceQueryResult(
                messageID??string.Empty,
                headers,
                messageTypeID??string.Empty,
                result.Data??new ReadOnlyMemory<byte>()
            );
        }

        /// <summary>
        /// Called to create a subscription to the underlying nats server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a message is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The queueGroup to use for the subscription</param>
        /// <param name="options">The service channel options, if desired, specifically the StreamPublishSubscriberOptions which is used to access streams vs standard subscription</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance</returns>
        /// <exception cref="InvalidChannelOptionsTypeException">Thrown when options is not null and is not an instance of the type StreamPublishSubscriberOptions</exception>
        public async ValueTask<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            InvalidChannelOptionsTypeException.ThrowIfNotNullAndNotOfType<StreamPublishChannelOptions>(options);
            IInternalServiceSubscription? subscription = null;
            if (options is StreamPublishSubscriberOptions subscriberOptions)
            {
                if (subscriberOptions.StreamConfig!=null)
                    await CreateStreamAsync(subscriberOptions.StreamConfig, cancellationToken);
                var consumer = await natsJSContext.CreateOrUpdateConsumerAsync(subscriberOptions.StreamConfig?.Name??channel, subscriberOptions.ConsumerConfig??new ConsumerConfig(group) { AckPolicy = ConsumerConfigAckPolicy.Explicit }, cancellationToken);
                subscription = new StreamSubscription(consumer, messageRecieved, errorRecieved, cancellationToken);
            }
            else
                subscription = new PublishSubscription(
                    natsConnection.SubscribeAsync<byte[]>(
                        channel,
                        queueGroup: group,
                        cancellationToken: cancellationToken
                    ),
                    messageRecieved,
                    errorRecieved,
                    cancellationToken
                );
            subscription.Run();
            return subscription;
        }

        /// <summary>
        /// Called to create a subscription for queries to the underlying NATS server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a query is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The queueGroup to use for the subscription</param>
        /// <param name="options">Should be null here as there is no Service Channel Options implemented for this call</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance</returns>
        /// /// <exception cref="NoChannelOptionsAvailableException">Thrown if options was supplied because there are no implemented options for this call</exception>
        public ValueTask<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, ValueTask<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            NoChannelOptionsAvailableException.ThrowIfNotNull(options);
            var sub = new QuerySubscription(
                natsConnection.SubscribeAsync<byte[]>(
                    channel,
                    queueGroup: group,
                    cancellationToken: cancellationToken
                ),
                messageRecieved,
                errorRecieved,
                cancellationToken
            );
            sub.Run();
            return ValueTask.FromResult<IServiceSubscription?>(sub);
        }

        /// <summary>
        /// Called to dispose of the object correctly and allow it to clean up it's resources
        /// </summary>
        /// <returns>A task required for disposal</returns>
        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await natsConnection.DisposeAsync();
                GC.SuppressFinalize(this);
            }
        }
    }
}
