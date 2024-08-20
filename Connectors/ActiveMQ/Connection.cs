using Apache.NMS;
using Apache.NMS.Util;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.ActiveMQ
{
    internal class Connection : IMessageServiceConnection
    {
        private const string MESSAGE_TYPE_HEADER_ID = "_MessageType";

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

        public Task<PingResult> PingAsync()
            => throw new NotImplementedException();

        private async Task<IBytesMessage> ProduceMessage(ServiceMessage message)
        {
            var msg = await session.CreateBytesMessageAsync(message.Data.ToArray());
            msg.NMSMessageId=message.ID;
            msg.Properties[MESSAGE_TYPE_HEADER_ID] = message.MessageTypeID;
            foreach (var key in message.Header.Keys)
                msg.Properties[key] = message.Header[key];
            return msg;
        }

        internal static RecievedServiceMessage ProduceMessage(string channel, IMessage message)
        {
            var headers = new Dictionary<string, string?>();
            foreach(var key in message.Properties.Keys.OfType<string>())
            {
                if (!Equals(key, MESSAGE_TYPE_HEADER_ID))
                    headers.Add(key, (string)message.Properties[key]);
            }
            return new(
                message.NMSMessageId,
                (string)message.Properties[MESSAGE_TYPE_HEADER_ID],
                channel,
                new(headers),
                message.Body<byte[]>()
            );
        }

        public async Task<TransmissionResult> PublishAsync(ServiceMessage message, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
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

        public Task<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, Task<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    producer.Dispose();
                    session.Dispose();
                    connection.Stop();
                    connection.Dispose();
                }
                disposedValue=true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
