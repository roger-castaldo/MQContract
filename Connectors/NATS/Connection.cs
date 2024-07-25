using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.NATS.Messages;
using MQContract.NATS.Options;
using MQContract.NATS.Serialization;
using MQContract.NATS.Subscriptions;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MQContract.NATS
{
    public class Connection : IMessageServiceConnection,IDisposable
    {
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

        public async Task<IPingResult> PingAsync()
            => new PingResponse(await natsConnection.PingAsync(), natsConnection.ServerInfo);

        public static NatsHeaders ExtractHeader(IMessageHeader header)
            => new(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(
                header.Keys.Select(k=>
                    new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>(
                        k,
                        new Microsoft.Extensions.Primitives.StringValues(header[k]))
                )
            ));

        public async Task<ITransmissionResult> PublishAsync(IServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null && options is not PublishChannelOptions)
                throw new InvalidChannelOptionsTypeException(typeof(PublishChannelOptions), options.GetType());
            try
            {
                if (options is PublishChannelOptions publishChannelOptions)
                {
                    if (publishChannelOptions.Config!=null)
                        await natsJSContext.CreateStreamAsync(publishChannelOptions.Config, cancellationToken);
                    var ack = await natsJSContext.PublishAsync<NatsMessage>(
                        message.Channel,
                        new NatsMessage()
                        {
                            ID=message.ID,
                            MessageTypeID=message.MessageTypeID,
                            Data=message.Data
                        },
                        headers: ExtractHeader(message.Header),
                        serializer: MessageSerializer<NatsMessage>.Default,
                        cancellationToken: cancellationToken
                    );
                    return new TransmissionResult(message.ID, (ack.Error!=null ? $"{ack.Error.Code}:{ack.Error.Description}" : null));
                }
                else
                {
                    await natsConnection.PublishAsync<NatsMessage>(
                        message.Channel,
                        new NatsMessage()
                        {
                            ID=message.ID,
                            MessageTypeID=message.MessageTypeID,
                            Data=message.Data
                        },
                        headers: ExtractHeader(message.Header),
                        serializer: MessageSerializer<NatsMessage>.Default,
                        cancellationToken: cancellationToken
                    );
                    return new TransmissionResult(message.ID);
                }
            }catch(Exception ex)
            {
                return new TransmissionResult(message.ID, ex.Message);
            }
        }

        public async Task<IServiceQueryResult> QueryAsync(IServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            var result = await natsConnection.RequestAsync<NatsMessage, NatsQueryResponseMessage>(
                message.Channel,
                new NatsMessage()
                {
                    ID=message.ID,
                    MessageTypeID=message.MessageTypeID,
                    Data=message.Data
                },
                headers: ExtractHeader(message.Header),
                requestSerializer: MessageSerializer<NatsMessage>.Default,
                replySerializer: MessageSerializer<NatsQueryResponseMessage>.Default,
                cancellationToken: cancellationToken
            );
            return new QueryResult(result);
        }

        public async Task<IServiceSubscription?> SubscribeAsync(Action<IRecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null && options is not PublishSubscriberOptions)
                throw new InvalidChannelOptionsTypeException(typeof(PublishSubscriberOptions), options.GetType());
            IInternalServiceSubscription? subscription = null;
            if (options is PublishSubscriberOptions subscriberOptions)
            {
                if (subscriberOptions.StreamConfig!=null)
                    await natsJSContext.CreateStreamAsync(subscriberOptions.StreamConfig, cancellationToken);
                var consumer = await natsJSContext.CreateOrUpdateConsumerAsync(subscriberOptions.StreamConfig?.Name??channel, subscriberOptions.ConsumerConfig??new ConsumerConfig(group) { AckPolicy = ConsumerConfigAckPolicy.Explicit }, cancellationToken);
                subscription = new StreamSubscription(consumer, messageRecieved, errorRecieved, cancellationToken);
            }
            else
                subscription = new PublishSubscription(
                    natsConnection.SubscribeAsync<NatsMessage>(
                        channel,
                        queueGroup: group,
                        serializer: MessageSerializer<NatsMessage>.Default,
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

        public async Task<IServiceSubscription?> SubscribeQueryAsync(Func<IRecievedServiceMessage, Task<IServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            var sub = new QuerySubscription(
                natsConnection.SubscribeAsync<NatsMessage>(
                    channel,
                    queueGroup: group,
                    serializer: MessageSerializer<NatsMessage>.Default,
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
