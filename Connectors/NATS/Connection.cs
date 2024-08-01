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
    public class Connection : IMessageServiceConnection,IDisposable
    {
        private const string MESSAGE_IDENTIFIER_HEADER = "_MessageID";
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private const string QUERY_RESPONSE_ERROR_TYPE = "NatsQueryError";

        private readonly NatsConnection natsConnection;
        private readonly NatsJSContext natsJSContext;
        private readonly ILogger? logger;
        private readonly List<IInternalServiceSubscription> subscriptions = [];
        private readonly SemaphoreSlim dataLock = new(1, 1);
        private bool disposedValue;

        private static NatsOpts FixOptions(NatsOpts options)
            => new()
            {
                Url=options.Url,
                Name=options.Name,
                Echo=options.Echo,
                Verbose=options.Verbose,
                Headers=options.Headers,
                AuthOpts=options.AuthOpts,
                TlsOpts=options.TlsOpts,
                SerializerRegistry=options.SerializerRegistry,
                LoggerFactory=options.LoggerFactory,
                WriterBufferSize=options.WriterBufferSize,
                ReaderBufferSize=options.ReaderBufferSize,
                UseThreadPoolCallback=options.UseThreadPoolCallback,
                InboxPrefix=options.InboxPrefix,
                NoRandomize=options.NoRandomize,
                PingInterval=options.PingInterval,
                MaxPingOut=options.MaxPingOut,
                ReconnectWaitMin=options.ReconnectWaitMin,
                ReconnectJitter=options.ReconnectJitter,
                ConnectTimeout=options.ConnectTimeout,
                ObjectPoolSize=options.ObjectPoolSize,
                RequestTimeout=options.RequestTimeout,
                CommandTimeout=options.CommandTimeout,
                SubscriptionCleanUpInterval=options.SubscriptionCleanUpInterval,
                HeaderEncoding=options.HeaderEncoding,
                SubjectEncoding=options.SubjectEncoding,
                WaitUntilSent=options.WaitUntilSent,
                MaxReconnectRetry=options.MaxReconnectRetry,
                ReconnectWaitMax=options.ReconnectWaitMax,
                IgnoreAuthErrorAbort=options.IgnoreAuthErrorAbort,
                SubPendingChannelCapacity=options.SubPendingChannelCapacity,
                SubPendingChannelFullMode=options.SubPendingChannelFullMode,
            };

        public Connection(NatsOpts options)
        {
            natsConnection = new(FixOptions(options));
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

        public int? MaxMessageBodySize { get; init; } = 1024*1024*1; //1MB default

        public TimeSpan DefaultTimout { get; init; } = TimeSpan.FromSeconds(5);

        public ValueTask<INatsJSStream> CreateStreamAsync(StreamConfig streamConfig,CancellationToken cancellationToken = default)
            => natsJSContext.CreateStreamAsync(streamConfig, cancellationToken);

        public async Task<PingResult> PingAsync()
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

        public async Task<TransmissionResult> PublishAsync(ServiceMessage message, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null && options is not StreamPublishChannelOptions)
                throw new InvalidChannelOptionsTypeException(typeof(StreamPublishChannelOptions), options.GetType());
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

        public async Task<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            var result = await natsConnection.RequestAsync<byte[], byte[]>(
                message.Channel,
                message.Data.ToArray(),
                headers: ExtractHeader(message),
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

        public async Task<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null && options is not StreamPublishSubscriberOptions)
                throw new InvalidChannelOptionsTypeException(typeof(StreamPublishSubscriberOptions), options.GetType());
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
            await dataLock.WaitAsync(cancellationToken);
            subscriptions.Add(subscription);
            dataLock.Release();
            return subscription;
        }

        public async Task<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, Task<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
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
            await dataLock.WaitAsync(cancellationToken);
            subscriptions.Add(sub);
            dataLock.Release();
            return sub;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dataLock.Wait();
                    foreach (var sub in subscriptions)
                        sub.EndAsync().Wait();
                    subscriptions.Clear();
                    Task.Run(async () => await natsConnection.DisposeAsync()).Wait();
                    dataLock.Release();
                    dataLock.Dispose();
                }
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
