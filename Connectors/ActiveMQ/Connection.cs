using Apache.NMS;
using Apache.NMS.Util;
using MQContract.ActiveMQ.Subscriptions;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.ActiveMQ
{
    internal class Connection : IMessageServiceConnection,IAsyncDisposable,IDisposable
    {
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private bool disposedValue;
        
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageProducer producer;

        public Connection(Uri ConnectUri){
            connectionFactory = new NMSConnectionFactory(ConnectUri);
            connection = connectionFactory.CreateConnection();
            connection.Start();
            session = connection.CreateSession();
            producer = session.CreateProducer();
        }

        public int? MaxMessageBodySize => 4*1024*1024;

        /// <summary>
        /// The default timeout to use for RPC calls when not specified by the class or in the call.
        /// DEFAULT:1 minute if not specified inside the connection options
        /// </summary>
        public TimeSpan DefaultTimout { get; init; } = TimeSpan.FromMinutes(1);

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
                .Where(h =>!Equals(h, MESSAGE_TYPE_HEADER)))
                result.Add(key, (string)properties[key]);
            return new(result);
        }

        internal static RecievedServiceMessage ProduceMessage(string channel, IMessage message)
        {
            var headers = ExtractHeaders(message.Properties, out var messageTypeID);
            return new(
                message.NMSMessageId,
                messageTypeID!,
                channel,
                headers,
                message.Body<byte[]>()
            );
        }

        public async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
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

        public async ValueTask<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            var result = new SubscriptionBase((msg)=>messageRecieved(ProduceMessage(channel,msg)), errorRecieved,session, channel, group);
            await result.StartAsync();
            return result;
        }

        public async ValueTask CloseAsync()
            => await connection.StopAsync();

        public async ValueTask DisposeAsync()
        {
            await connection.StopAsync().ConfigureAwait(true);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
