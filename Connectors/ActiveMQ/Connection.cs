using Apache.NMS;
using Apache.NMS.Util;
using MQContract.ActiveMQ.Subscriptions;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MQContract.ActiveMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using ActiveMQ
    /// </summary>
    public sealed class Connection : IInboxQueryableMessageServiceConnection, IPingableMessageServiceConnection, IAsyncDisposable
    {
        private readonly record struct MessageInstance(string ID, IBytesMessage Message, IDestination Topic);

        private const string MESSAGE_TYPE_HEADER = "x-mqcontract-message-type";
        private const string MESSAGE_CORRELATION_ID_HEADER = "x-mqcontract-correlation-id";
        private bool disposedValue;

        private readonly ISession session;
        private readonly IMessageProducer producer;
        private readonly ConcurrentDictionary<(string channel, string group), ConsumerInstance> consumerInstances = [];
        private readonly ConcurrentDictionary<string, ITopic> topicMap = [];
        private readonly BatchedMessageStream<MessageInstance> batchedMessageStream;
        private readonly ITopic inboxTopic;

        /// <summary>
        /// Underlying connection used to connection to ActiveMQ.  Exposed here for additional control if required.
        /// </summary>
        public IConnection ActiveMQConnection { get; private init; }

        /// <summary>
        /// Default constructor for creating instance
        /// </summary>
        /// <param name="ConnectUri">The connection url to use</param>
        /// <param name="username">The username to use</param>
        /// <param name="password">The password to use</param>
        public Connection(Uri ConnectUri, string username, string password)
        {
            var connectionFactory = new NMSConnectionFactory(ConnectUri);
            ActiveMQConnection = connectionFactory.CreateConnection(username, password);
            ActiveMQConnection.Start();
            session = ActiveMQConnection.CreateSession();
            producer = session.CreateProducer();
            inboxTopic = SessionUtil.GetTopic(session, $"inbox_{Guid.NewGuid()}");
            batchedMessageStream = new(
                (serviceMessage, _) => ProduceMessageAsync(serviceMessage),
                async (messageInstance, cancellationToken) =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return new TransmissionResult(messageInstance.ID, Error: new(new OperationCanceledException("Transmission cancelled"), true));
                        await producer.SendAsync(messageInstance.Topic, messageInstance.Message);
                        return new TransmissionResult(messageInstance.ID);
                    }
                    catch (Exception ex)
                    {
                        return new TransmissionResult(messageInstance.ID, Error: new(ex, ex switch
                        {
                            IllegalStateException => true,
                            InvalidDestinationException => true,
                            MessageFormatException => true,
                            _ => false
                        }));
                    }
                });
        }

        private async ValueTask<MessageInstance> ProduceMessageAsync(ServiceMessage serviceMessage, IDestination? replyTo = null, Guid? correlationID = null)
        {
            var msg = await session.CreateBytesMessageAsync(serviceMessage.Data.ToArray());
            msg.NMSMessageId=serviceMessage.ID;
            msg.Properties[MESSAGE_TYPE_HEADER] = serviceMessage.MessageTypeID;
            if (replyTo!=null)
                msg.NMSReplyTo = replyTo;
            if (correlationID!=null)
                msg.Properties[MESSAGE_CORRELATION_ID_HEADER] = correlationID.ToString();
            serviceMessage.Header.ForEach(pair => msg.Properties[pair.Key] = pair.Value);
            return new MessageInstance(serviceMessage.ID, msg, GetTopic(serviceMessage.Channel));
        }

        uint? IMessageServiceConnection.MaxMessageBodySize => 4*1024*1024;

        /// <summary>
        /// The default timeout to use for RPC calls when not specified by class or in the call.
        /// DEFAULT: 1 minute
        /// </summary>
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromMinutes(1);

        private static MessageHeader ExtractHeaders(IPrimitiveMap properties, out string? messageTypeID, out Guid? correlationID)
        {
            messageTypeID = (string?)(properties.Contains(MESSAGE_TYPE_HEADER) ? properties[MESSAGE_TYPE_HEADER] : null);
            correlationID = (properties.Contains(MESSAGE_CORRELATION_ID_HEADER) ? new((string)properties[MESSAGE_CORRELATION_ID_HEADER]) : null);
            return new(properties.Keys.OfType<string>()
                .Where(h => !Equals(h, MESSAGE_TYPE_HEADER) && !Equals(h, MESSAGE_CORRELATION_ID_HEADER))
                .Select(key => new KeyValuePair<string, string?>(key, (string?)properties[key])));
        }

        internal static ReceivedServiceMessage ProduceMessage(string channel, IMessage message, TaskCompletionSource ackSource, out Guid? correlationID)
        {
            var headers = ExtractHeaders(message.Properties, out var messageTypeID, out correlationID);
            return new(
                message.NMSMessageId,
                messageTypeID!,
                channel,
                headers,
                message.Body<byte[]>(),
                async () =>
                {
                    await message.AcknowledgeAsync();
                    ackSource.TrySetResult();
                }
            );
        }

        internal static ReceivedInboxServiceMessage ProduceInboxMessage(string channel, IMessage message, TaskCompletionSource ackSource)
        {
            var headers = ExtractHeaders(message.Properties, out var messageTypeID, out var correlationID);
            return new(
                message.NMSMessageId,
                messageTypeID!,
                channel,
                headers,
                correlationID??Guid.Empty,
                message.Body<byte[]>(),
                async () =>
                {
                    await message.AcknowledgeAsync();
                    ackSource.TrySetResult();
                }
            );
        }

        private ITopic GetTopic(string channel)
        {
            if (!topicMap.TryGetValue(channel, out var topic))
            {
                topic = SessionUtil.GetTopic(session, channel);
                topicMap.TryAdd(channel, topic);
            }
            return topic;
        }

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(message, cancellationToken);

        ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(messages, cancellationToken);

        private async ValueTask<ConsumerInstance> CreateInstance(string channel, string group)
        {
            if (!consumerInstances.TryGetValue((channel, group), out var result))
            {
                result = new(await session.CreateSharedConsumerAsync(GetTopic(channel), group), () =>
                {
                    consumerInstances.TryRemove((channel, group), out _);
                });
                consumerInstances.TryAdd((channel, group), result);
            }
            else
                result.AddListener();
            return result;
        }

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            group??=Guid.NewGuid().ToString();
            var result = new SubscriptionBase(
                (msg,ackSource) => messageReceived(ProduceMessage(channel, msg, ackSource, out _)), 
                errorReceived, 
                await CreateInstance(channel, group)
            );
            result.Start();
            return result;
        }

        async ValueTask IMessageServiceConnection.CloseAsync()
            => await ((IAsyncDisposable)this).DisposeAsync();

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            try
            {
                var start = Stopwatch.GetTimestamp();
                using var sess = await ActiveMQConnection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
                using var tempQueue = await sess.CreateTemporaryQueueAsync();
                return new(ActiveMQConnection.MetaData.NMSProviderName, ActiveMQConnection.MetaData.NMSVersion, Stopwatch.GetElapsedTime(start));
            }
            catch
            {
                throw new PingFailedException("Unable to create a temporary session or queue in order to ping ActiveMQ instance");
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await batchedMessageStream.DisposeAsync().ConfigureAwait(true);
                await ActiveMQConnection.StopAsync().ConfigureAwait(true);
                producer.Dispose();
                try
                {
                    await session.DeleteDestinationAsync(inboxTopic);
                }
                catch { 
                    //bury error as failure is not important
                }
                session.Dispose();
                ActiveMQConnection.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        async ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Func<ReceivedInboxServiceMessage, ValueTask> messageReceived, CancellationToken cancellationToken)
        {
            var result = new SubscriptionBase(
                (msg, ackSource) => messageReceived(ProduceInboxMessage(inboxTopic.TopicName, msg, ackSource)),
                (err) => { },
                new(await session.CreateConsumerAsync(inboxTopic), () => { })
            );
            result.Start();
            return result;
        }

        async ValueTask<TransmissionResult> IInboxQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, Guid correlationID, CancellationToken cancellationToken)
        {
            var messageInstance = await ProduceMessageAsync(message, replyTo: inboxTopic, correlationID: correlationID);
            await producer.SendAsync(messageInstance.Topic, messageInstance.Message);
            return new TransmissionResult(messageInstance.ID);
        }

        async ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            group??=Guid.NewGuid().ToString();
            var result = new SubscriptionBase(
                async (msg, ackSource) => {
                    var result = await messageReceived(ProduceMessage(channel, msg, ackSource, out var correlationID));
                    if (result!=null)
                    {
                        var messageInstance = await ProduceMessageAsync(result, correlationID: correlationID);
                        await producer.SendAsync(msg.NMSReplyTo, messageInstance.Message);
                    }
                },
                errorReceived,
                await CreateInstance(channel, group)
            );
            result.Start();
            return result;
        }
    }
}
