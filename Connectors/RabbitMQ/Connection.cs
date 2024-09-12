using MQContract.Interfaces.Service;
using MQContract.Messages;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.RabbitMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using RabbitMQ
    /// </summary>
    public class Connection : IMessageServiceConnection,IDisposable
    {
        private readonly IConnection conn;
        private readonly IModel channel;
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private bool disposedValue;

        /// <summary>
        /// Default constructor for creating instance
        /// </summary>
        /// <param name="factory">The connection factory to use that was built with required authentication and connection information</param>
        public Connection(ConnectionFactory factory)
        {
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            MaxMessageBodySize = factory.MaxMessageSize;
        }

        /// <summary>
        /// The maximum message body size allowed
        /// </summary>
        public uint? MaxMessageBodySize { get; private init; }

        public ValueTask CloseAsync()
        {
            Dispose(true);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Called to publish a message into the ActiveMQ server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Transmition result identifying if it worked or not</returns>
        public async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken);
            TransmissionResult result;
            try
            {
                var props = channel.CreateBasicProperties();
                props.MessageId = message.MessageTypeID;
                props.Type = message.MessageTypeID;
                foreach(var key in message.Header.Keys)
                    props.Headers.Add(key, message.Header[key]);
                channel.BasicPublish(string.Empty, message.Channel,props, message.Data);
                result = new TransmissionResult(message.ID);
            }catch(Exception e)
            {
                result = new TransmissionResult(message.ID, e.Message);
            }
            semaphore.Release();
            return result;
        }

        /// <summary>
        /// Called to create a subscription to the underlying RabbitMQ server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a message is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValueTask<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string? group = null, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<IServiceSubscription?>(new Subscription(conn,channel,group, messageRecieved, errorRecieved));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    semaphore.Wait();
                    channel.Close();
                    channel.Dispose();
                    conn.Close();
                    conn.Dispose();
                    semaphore.Release();
                    semaphore.Dispose();
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
