using MQContract.Interfaces.Service;
using MQContract.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MQContract.RabbitMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using RabbitMQ
    /// </summary>
    public sealed class Connection : IQueryableMessageServiceConnection, IDisposable
    {
        private readonly ConnectionFactory factory;
        private readonly IConnection conn;
        private readonly IModel channel;
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly Dictionary<Guid, TaskCompletionSource<ServiceQueryResult>> awaitingResponses = [];
        private IModel? responseListener;
        private string? responseListenerTag;
        private bool disposedValue;

        /// <summary>
        /// Default constructor for creating instance
        /// </summary>
        /// <param name="factory">The connection factory to use that was built with required authentication and connection information</param>
        public Connection(ConnectionFactory factory)
        {
            this.factory = factory;
            if (string.IsNullOrWhiteSpace(this.factory.ClientProvidedName))
                this.factory.ClientProvidedName = Guid.NewGuid().ToString();
            conn = this.factory.CreateConnection();
            channel = conn.CreateModel();
            MaxMessageBodySize = factory.MaxMessageSize;
        }

        /// <summary>
        /// Used to declare a queue inside the RabbitMQ server
        /// </summary>
        /// <param name="queue">The name of the queue</param>
        /// <param name="durable">Is this queue durable</param>
        /// <param name="exclusive">Is this queue exclusive</param>
        /// <param name="autoDelete">Auto Delete queue when connection closed</param>
        /// <param name="arguments">Additional arguements</param>
        /// <returns>The connection to allow for chaining calls</returns>
        public Connection QueueDeclare(string queue, bool durable = false, bool exclusive = false,
            bool autoDelete = true, IDictionary<string, object>? arguments = null)
        {
            channel.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
            return this;
        }

        /// <summary>
        /// Used to decalre an exchange inside the RabbitMQ server
        /// </summary>
        /// <param name="exchange">The name of the exchange</param>
        /// <param name="type">The type of the exchange</param>
        /// <param name="durable">Is this durable</param>
        /// <param name="autoDelete">Auto Delete when connection closed</param>
        /// <param name="arguments">Additional arguements</param>
        /// <returns>The connection to allow for chaining calls</returns>
        public Connection ExchangeDeclare(string exchange, string type, bool durable = false, bool autoDelete = false,
            IDictionary<string, object>? arguments = null)
        {
            channel.ExchangeDeclare(exchange,type,durable,autoDelete,arguments);
            return this;
        }

        /// <summary>
        /// Used to delete a queue inside the RabbitMQ server
        /// </summary>
        /// <param name="queue">The name of the queue</param>
        /// <param name="ifUnused">Is unused</param>
        /// <param name="ifEmpty">Is Empty</param>
        public void QueueDelete(string queue, bool ifUnused, bool ifEmpty)
            => channel.QueueDelete(queue,ifUnused,ifEmpty);

        /// <summary>
        /// The maximum message body size allowed
        /// </summary>
        public uint? MaxMessageBodySize { get; init; }

        /// <summary>
        /// The default timeout to use for RPC calls when not specified by class or in the call.
        /// DEFAULT: 1 minute
        /// </summary>
        public TimeSpan DefaultTimout { get; init; } = TimeSpan.FromMinutes(1);

        internal static (IBasicProperties props, ReadOnlyMemory<byte>) ConvertMessage(ServiceMessage message, IModel channel, Guid? messageId = null)
        {
            var props = channel.CreateBasicProperties();
            props.MessageId=message.ID;
            props.Type = message.MessageTypeID;
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            if (messageId!=null)
            {
                bw.Write((byte)1);
                bw.Write(messageId.Value.ToByteArray());
            }
            else
                bw.Write((byte)0);
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

        internal static ReceivedServiceMessage ConvertMessage(BasicDeliverEventArgs eventArgs,string channel, Func<ValueTask> acknowledge,out Guid? messageId)
        {
            using var ms = new MemoryStream(eventArgs.Body.ToArray());
            using var br = new BinaryReader(ms);
            var flag = br.ReadByte();
            if (flag==1)
                messageId = new Guid(br.ReadBytes(16));
            else
                messageId=null;
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
       
        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
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

        private Subscription ProduceSubscription(IConnection conn, string channel, string? group, Action<BasicDeliverEventArgs,IModel, Func<ValueTask>> messageReceived, Action<Exception> errorReceived)
        {
            if (group==null)
            {
                group = Guid.NewGuid().ToString();
                this.channel.QueueDeclare(queue:group, durable:false, exclusive:false, autoDelete:true);
            }
            return new Subscription(conn, channel, group,messageReceived,errorReceived);
        }

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => ValueTask.FromResult<IServiceSubscription?>(ProduceSubscription(conn, channel, group,
                (@event,modelChannel, acknowledge) =>
                {
                    messageReceived(ConvertMessage(@event, channel, acknowledge,out _));
                },
                errorReceived
            ));

        private const string InboxChannel = "_Inbox";

        async ValueTask<ServiceQueryResult> IQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var messageID = Guid.NewGuid();
            (var props, var data) = ConvertMessage(message, this.channel,messageID);
            props.ReplyTo = $"{InboxChannel}.${factory.ClientProvidedName}";
            var success = true;
            await semaphore.WaitAsync(cancellationToken);
            var result = new TaskCompletionSource<ServiceQueryResult>();
            awaitingResponses.Add(messageID, result);
            if (responseListener==null)
            {
                ExchangeDeclare(InboxChannel, ExchangeType.Direct);
                QueueDeclare(props.ReplyTo);
                responseListener = this.conn.CreateModel();
                responseListener.QueueBind(props.ReplyTo, InboxChannel, props.ReplyTo);
                responseListener.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(responseListener!);
                consumer.Received+=(sender, @event) =>
                {
                    var responseMessage = ConvertMessage(@event, string.Empty, () => ValueTask.CompletedTask, out var messageId);
                    if (messageId!=null)
                    {
                        semaphore.Wait();
                        if (awaitingResponses.TryGetValue(messageId.Value, out var taskCompletionSource))
                        {
                            taskCompletionSource.TrySetResult(new(
                                responseMessage.ID,
                                responseMessage.Header,
                                responseMessage.MessageTypeID,
                                responseMessage.Data
                            ));
                            responseListener.BasicAck(@event.DeliveryTag, false);
                            awaitingResponses.Remove(messageId.Value);
                        }
                        semaphore.Release();
                    }
                };
                responseListenerTag = responseListener.BasicConsume(props.ReplyTo, false, consumer);
            }
            try
            {
                channel.BasicPublish(message.Channel, string.Empty, props, data);
            }
            catch (Exception)
            {
                success=false;
            }
            if (!success)
                awaitingResponses.Remove(messageID);
            semaphore.Release();
            if (!success)
                throw new InvalidOperationException("An error occured attempting to submit the requested query");
            await result.Task.WaitAsync(timeout, cancellationToken);
            if (result.Task.IsCompleted)
                return result.Task.Result;
            throw new TimeoutException();
        }

        ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
         => ValueTask.FromResult<IServiceSubscription?>(ProduceSubscription(conn, channel, group,
                async (@event,model, acknowledge) =>
                {
                    var result = await messageReceived(ConvertMessage(@event, channel, acknowledge,out var messageID));
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        (var props, var data) = ConvertMessage(result,model,messageID);
                        this.channel.BasicPublish(InboxChannel, @event.BasicProperties.ReplyTo, props, data);
                    }
                    catch (Exception e)
                    {
                        errorReceived(e);
                    }
                    semaphore.Release();
                },
                errorReceived
            ));

        ValueTask IMessageServiceConnection.CloseAsync()
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
                    if (responseListener!=null)
                    {
                        responseListener.BasicCancel(responseListenerTag);
                        responseListener.Close();
                    }
                    conn.Close();
                    conn.Dispose();
                    semaphore.Release();
                    semaphore.Dispose();
                }
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
