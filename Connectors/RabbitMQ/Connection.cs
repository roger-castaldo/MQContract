using MQContract.Interfaces.Service;
using MQContract.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

        public Connection QueueDeclare(string queue, bool durable = false, bool exclusive = false,
            bool autoDelete = true, IDictionary<string, object>? arguments = null)
        {
            channel.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
            return this;
        }

        public Connection ExchangeDeclare(string exchange, string type, bool durable = false, bool autoDelete = false,
            IDictionary<string, object> arguments = null)
        {
            channel.ExchangeDeclare(exchange,type,durable,autoDelete,arguments);
            return this;
        }

        public void QueueDelete(string queue, bool ifUnused, bool ifEmpty)
            => channel.QueueDelete(queue,ifUnused,ifEmpty);

        /// <summary>
        /// The maximum message body size allowed
        /// </summary>
        public uint? MaxMessageBodySize { get; init; }

        internal static (IBasicProperties props,ReadOnlyMemory<byte>) ConvertMessage(ServiceMessage message,IModel channel)
        {
            var props = channel.CreateBasicProperties();
            props.MessageId=message.ID;
            props.Type = message.MessageTypeID;
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write(message.Data.Length);
            bw.Write(message.Data.ToArray());
            foreach(var key in message.Header.Keys)
            {
                var bytes = UTF8Encoding.UTF8.GetBytes(key);
                bw.Write(bytes.Length);
                bw.Write(bytes);
                bytes = UTF8Encoding.UTF8.GetBytes(message.Header[key]!);
                bw.Write(bytes.Length);
                bw.Write(bytes);
            }
            bw.Flush();
            return (props, ms.ToArray());
        }

        internal static RecievedServiceMessage ConvertMessage(BasicDeliverEventArgs eventArgs,string channel, Func<ValueTask> acknowledge)
        {
            using var ms = new MemoryStream(eventArgs.Body.ToArray());
            using var br = new BinaryReader(ms);
            var data = br.ReadBytes(br.ReadInt32());
            var header = new Dictionary<string, string?>();
            while (br.BaseStream.Position<br.BaseStream.Length)
            {
                var key = UTF8Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32()));
                var value = UTF8Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32()));
                header.Add(key, value);
            }
            return new(
                eventArgs.BasicProperties.MessageId,
                eventArgs.BasicProperties.Type,
                channel,
                new(header),
                data.ToArray(),
                acknowledge
            );
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
                (var props, var data) = ConvertMessage(message, this.channel);
                channel.BasicPublish(message.Channel,string.Empty,props,data);
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
            if (group==null)
            {
                group = Guid.NewGuid().ToString();
                this.channel.QueueDeclare(group,true,false,false);
            }
            return ValueTask.FromResult<IServiceSubscription?>(new Subscription(conn,channel,group, messageRecieved, errorRecieved));
        }

        public ValueTask CloseAsync()
        {
            Dispose(true);
            return ValueTask.CompletedTask;
        }

        private void Dispose(bool disposing)
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
