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
    public sealed class Connection : IPingableMessageServiceConnection, IAsyncDisposable
    {
        private readonly record struct MessageInstance(string ID, IBytesMessage Message, IDestination Topic);

        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private bool disposedValue;

        private readonly ISession session;
        private readonly IMessageProducer producer;
        private readonly ConcurrentDictionary<(string channel, string group), ConsumerInstance> consumerInstances = [];
        private readonly ConcurrentDictionary<string, ITopic> topicMap = [];
        private readonly BatchedMessageStream<MessageInstance> batchedMessageStream;

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
            batchedMessageStream = new(
                async (serviceMessage, _) =>
                {
                    var msg = await session.CreateBytesMessageAsync(serviceMessage.Data.ToArray());
                    msg.NMSMessageId=serviceMessage.ID;
                    msg.Properties[MESSAGE_TYPE_HEADER] = serviceMessage.MessageTypeID;
                    serviceMessage.Header.ForEach(pair => msg.Properties[pair.Key] = pair.Value);
                    return new MessageInstance(serviceMessage.ID, msg, GetTopic(serviceMessage.Channel));
                },
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

        uint? IMessageServiceConnection.MaxMessageBodySize => 4*1024*1024;

        private static MessageHeader ExtractHeaders(IPrimitiveMap properties, out string? messageTypeID)
        {
            messageTypeID = (string?)(properties.Contains(MESSAGE_TYPE_HEADER) ? properties[MESSAGE_TYPE_HEADER] : null);
            return new(properties.Keys.OfType<string>()
                .Where(h => !Equals(h, MESSAGE_TYPE_HEADER))
                .Select(key => new KeyValuePair<string, string?>(key, (string?)properties[key])));
        }

        internal static ReceivedServiceMessage ProduceMessage(string channel, IMessage message, TaskCompletionSource ackSource)
        {
            var headers = ExtractHeaders(message.Properties, out var messageTypeID);
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
            var result = new SubscriptionBase((msg,ackSource) => messageReceived(ProduceMessage(channel, msg, ackSource)), errorReceived, await CreateInstance(channel, group));
            await result.StartAsync();
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
                session.Dispose();
                ActiveMQConnection.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
