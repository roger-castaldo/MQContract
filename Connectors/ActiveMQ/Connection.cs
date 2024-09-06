﻿using Apache.NMS;
using Apache.NMS.Util;
using MQContract.ActiveMQ.Subscriptions;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.ActiveMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using ActiveMQ
    /// </summary>
    public class Connection : IMessageServiceConnection,IAsyncDisposable,IDisposable
    {
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private bool disposedValue;
        
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageProducer producer;

        /// <summary>
        /// Default constructor for creating instance
        /// </summary>
        /// <param name="ConnectUri">The connection url to use</param>
        /// <param name="username">The username to use</param>
        /// <param name="password">The password to use</param>
        public Connection(Uri ConnectUri,string username,string password){
            var connectionFactory = new NMSConnectionFactory(ConnectUri);
            connection = connectionFactory.CreateConnection(username,password);
            connection.Start();
            session = connection.CreateSession();
            producer = session.CreateProducer();
        }

        /// <summary>
        /// The maximum message body size allowed
        /// </summary>
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

        /// <summary>
        /// Called to publish a message into the ActiveMQ server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="options">The service channel options which should be null as there is no implementations for ActiveMQ</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Transmition result identifying if it worked or not</returns>
        /// <exception cref="NoChannelOptionsAvailableException">Thrown if options was supplied because there are no implemented options for this call</exception>
        public async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            NoChannelOptionsAvailableException.ThrowIfNotNull(options);
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

        /// <summary>
        /// Called to create a subscription to the underlying ActiveMQ server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a message is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The group to subscribe as part of</param>
        /// <param name="options">should be null</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NoChannelOptionsAvailableException">Thrown if options was supplied because there are no implemented options for this call</exception>
        public async ValueTask<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            NoChannelOptionsAvailableException.ThrowIfNotNull(options);
            var result = new SubscriptionBase((msg)=>messageRecieved(ProduceMessage(channel,msg)), errorRecieved,session, channel, group);
            await result.StartAsync();
            return result;
        }

        /// <summary>
        /// Called to close off the underlying ActiveMQ Connection
        /// </summary>
        /// <returns></returns>
        public async ValueTask CloseAsync()
            => await connection.StopAsync();

        /// <summary>
        /// Called to dispose of the object correctly and allow it to clean up it's resources
        /// </summary>
        /// <returns>A task required for disposal</returns>
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

        /// <summary>
        /// Called to dispose of the required resources
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
