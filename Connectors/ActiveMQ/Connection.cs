using Apache.NMS;
using Apache.NMS.Util;
using MQContract.ActiveMQ.Subscriptions;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.ActiveMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using ActiveMQ
    /// </summary>
    public sealed class Connection : IMessageServiceConnection, IAsyncDisposable, IDisposable
    {
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private bool disposedValue;

        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageProducer producer;
        private readonly List<ConsumerInstance> consumerInstances = [];
        private readonly SemaphoreSlim locker = new(1, 1);

        /// <summary>
        /// Default constructor for creating instance
        /// </summary>
        /// <param name="ConnectUri">The connection url to use</param>
        /// <param name="username">The username to use</param>
        /// <param name="password">The password to use</param>
        public Connection(Uri ConnectUri, string username, string password)
        {
            var connectionFactory = new NMSConnectionFactory(ConnectUri);
            connection = connectionFactory.CreateConnection(username, password);
            connection.Start();
            session = connection.CreateSession();
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
                return new TransmissionResult(message.ID, Error: ex.Message);
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
            => await connection.StopAsync();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await connection.StopAsync().ConfigureAwait(true);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    connection.Stop();

                producer.Dispose();
                session.Dispose();
                connection.Dispose();
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
