using Apache.NMS;
using Apache.NMS.Util;
using MQContract.ActiveMQ.Subscriptions;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.ActiveMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using ActiveMQ
    /// </summary>
    public sealed class Connection : IPingableMessageServiceConnection, IAsyncDisposable, IDisposable
    {
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private bool disposedValue;

        private readonly ISession session;
        private readonly IMessageProducer producer;
        private readonly List<ConsumerInstance> consumerInstances = [];
        private readonly SemaphoreSlim locker = new(1, 1);

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
        }

        uint? IMessageServiceConnection.MaxMessageBodySize => 4*1024*1024;

        private async ValueTask<IBytesMessage> ProduceMessage(ServiceMessage message)
        {
            var msg = await session.CreateBytesMessageAsync(message.Data.ToArray());
            msg.NMSMessageId=message.ID;
            msg.Properties[MESSAGE_TYPE_HEADER] = message.MessageTypeID;
            foreach (var key in message.Header.Keys)
                msg.Properties[key] = message.Header[key];
            return msg;
        }

        private static MessageHeader ExtractHeaders(IPrimitiveMap properties, out string? messageTypeID)
        {
            var result = new Dictionary<string, string?>();
            messageTypeID = (string?)(properties.Contains(MESSAGE_TYPE_HEADER) ? properties[MESSAGE_TYPE_HEADER] : null);
            foreach (var key in properties.Keys.OfType<string>()
                .Where(h => !Equals(h, MESSAGE_TYPE_HEADER)))
                result.Add(key, (string)properties[key]);
            return new(result);
        }

        internal static ReceivedServiceMessage ProduceMessage(string channel, IMessage message)
        {
            var headers = ExtractHeaders(message.Properties, out var messageTypeID);
            return new(
                message.NMSMessageId,
                messageTypeID!,
                channel,
                headers,
                message.Body<byte[]>(),
                async () => await message.AcknowledgeAsync()
            );
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            try
            {
                await producer.SendAsync(SessionUtil.GetTopic(session, message.Channel), await ProduceMessage(message));
                return new TransmissionResult(message.ID);
            }
            catch (Exception ex)
            {
                return new TransmissionResult(message.ID, Error: new(ex, ex switch
                {
                    IllegalStateException => true,
                    InvalidDestinationException => true,
                    MessageFormatException => true,
                    _ => false
                }));
            }
        }

        private async ValueTask<ConsumerInstance> CreateInstance(string channel, string group)
        {
            await locker.WaitAsync();
            var result = consumerInstances.Find(x => Equals(x.Channel, channel) && Equals(x.Group, group));
            locker.Release();
            if (result==null)
            {
                await locker.WaitAsync();
                result = new(channel, group, await session.CreateSharedConsumerAsync(SessionUtil.GetTopic(session, channel), group), () =>
                {
                    locker.Wait();
                    consumerInstances.RemoveAll(x => Equals(x.Channel, channel) && Equals(x.Group, group));
                    locker.Release();
                });
                consumerInstances.Add(result);
                locker.Release();
            }
            else
                result.AddListener();
            return result;
        }

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            group??=Guid.NewGuid().ToString();
            var result = new SubscriptionBase((msg) => messageReceived(ProduceMessage(channel, msg)), errorReceived, await CreateInstance(channel, group));
            await result.StartAsync();
            return result;
        }

        async ValueTask IMessageServiceConnection.CloseAsync()
            => await ActiveMQConnection.StopAsync();

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

        private void DisposeComponents()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                producer.Dispose();
                session.Dispose();
                ActiveMQConnection.Dispose();
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
                await ActiveMQConnection.StopAsync().ConfigureAwait(true);

            DisposeComponents();
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            if (!disposedValue)
            {
                ActiveMQConnection.Stop();
                DisposeComponents();
            }
            GC.SuppressFinalize(this);
        }
    }
}
