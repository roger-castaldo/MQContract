using MQContract.Interfaces.Service;
using MQContract.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;

namespace MQContract.RabbitMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using RabbitMQ
    /// </summary>
    public sealed class Connection : IInboxQueryableMessageServiceConnection, IAsyncDisposable
    {
        private const string InboxExchange = "_Inbox";

        private readonly IConnection conn;
        private readonly IChannel channel;
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly string inboxChannel;
        private bool disposedValue;

        /// <summary>
        /// Default constructor for creating instance
        /// </summary>
        /// <param name="factory">The connection factory to use that was built with required authentication and connection information</param>
        public Connection(ConnectionFactory factory)
        {
            if (string.IsNullOrWhiteSpace(factory.ClientProvidedName))
                factory.ClientProvidedName = Guid.NewGuid().ToString();
            var connectionTask = factory.CreateConnectionAsync();
            connectionTask.Wait();
            conn = connectionTask.Result;
            var channelTask = conn.CreateChannelAsync();
            channelTask.Wait();
            channel = channelTask.Result;
            MaxMessageBodySize = factory.MaxInboundMessageBodySize;
            inboxChannel = $"{InboxExchange}.{factory.ClientProvidedName}";
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
        public async Task<Connection> QueueDeclareAsync(string queue, bool durable = false, bool exclusive = false,
            bool autoDelete = true, IDictionary<string, object?>? arguments = null)
        {
            await channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, arguments: arguments);
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
        public async Task<Connection> ExchangeDeclareAsync(string exchange, string type, bool durable = false, bool autoDelete = false,
            IDictionary<string, object?>? arguments = null)
        {
            await channel.ExchangeDeclareAsync(exchange, type, durable, autoDelete, arguments);
            return this;
        }

        /// <summary>
        /// Used to delete a queue inside the RabbitMQ server
        /// </summary>
        /// <param name="queue">The name of the queue</param>
        /// <param name="ifUnused">Is unused</param>
        /// <param name="ifEmpty">Is Empty</param>
        public async Task QueueDeleteAsync(string queue, bool ifUnused, bool ifEmpty)
            => await channel.QueueDeclareAsync(queue, ifUnused, ifEmpty);

        /// <summary>
        /// The maximum message body size allowed
        /// </summary>
        public uint? MaxMessageBodySize { get; init; }

        /// <summary>
        /// The default timeout to use for RPC calls when not specified by class or in the call.
        /// DEFAULT: 1 minute
        /// </summary>
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromMinutes(1);

        internal static (BasicProperties props, ReadOnlyMemory<byte>) ConvertMessage(ServiceMessage message, Guid? messageId = null)
        {
            var props = new BasicProperties
            {
                MessageId=message.ID,
                Type = message.MessageTypeID
            };
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
            foreach (var key in message.Header.Keys)
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

        internal static ReceivedServiceMessage ConvertMessage(BasicDeliverEventArgs eventArgs, string channel, Func<ValueTask> acknowledge, out Guid? messageId)
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
                eventArgs.BasicProperties.MessageId!,
                eventArgs.BasicProperties.Type!,
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
                (var props, var data) = ConvertMessage(message);
                await channel.BasicPublishAsync<BasicProperties>(message.Channel, string.Empty, true, props, data, cancellationToken);
                result = new TransmissionResult(message.ID);
            }
            catch (Exception e)
            {
                result = new TransmissionResult(message.ID, Error: new(e,e switch
                {
                    PublishException => true,
                    _ => false
                }));
            }
            semaphore.Release();
            return result;
        }

        private async Task<Subscription> ProduceSubscriptionAsync(IConnection conn, string channel, string? group, Action<BasicDeliverEventArgs, IChannel, Func<ValueTask>> messageReceived, Action<Exception> errorReceived)
        {
            if (group==null)
            {
                group = Guid.NewGuid().ToString();
                await this.channel.QueueDeclareAsync(queue: group, durable: false, exclusive: false, autoDelete: true);
            }
            else
            {
                try
                {
                    await this.channel.QueueDeclareAsync(queue: group);
                }
                catch (Exception)
                {
                    //this may throw an error is the queue already exists but checking for it fails
                }
            }
            return await Subscription.ProduceInstanceAsync(conn, channel, group, messageReceived, errorReceived);
        }

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => await ProduceSubscriptionAsync(conn, channel, group,
                (@event, modelChannel, acknowledge) =>
                {
                    messageReceived(ConvertMessage(@event, channel, acknowledge, out _));
                },
                errorReceived
            );

        async ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Action<ReceivedInboxServiceMessage> messageReceived, CancellationToken cancellationToken)
        {
            await channel.ExchangeDeclareAsync(InboxExchange, ExchangeType.Direct, durable: false, autoDelete: true, cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(inboxChannel, durable: false, exclusive: false, autoDelete: true, cancellationToken: cancellationToken);
            return await Subscription.ProduceInstanceAsync(
                conn,
                InboxExchange,
                inboxChannel,
                (@event, model, acknowledge) =>
                {
                    var responseMessage = ConvertMessage(@event, string.Empty, acknowledge, out var messageId);
                    if (messageId!=null)
                        messageReceived(new(
                            responseMessage.ID,
                            responseMessage.MessageTypeID,
                            inboxChannel,
                            responseMessage.Header,
                            messageId.Value,
                            responseMessage.Data,
                            acknowledge
                        ));
                },
                (error) => { },
                routingKey: inboxChannel
            );
        }

        async ValueTask<TransmissionResult> IInboxQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, Guid correlationID, CancellationToken cancellationToken)
        {
            (var props, var data) = ConvertMessage(message, correlationID);
            props.ReplyTo = inboxChannel;
            await semaphore.WaitAsync(cancellationToken);
            TransmissionResult result;
            try
            {
                await channel.BasicPublishAsync<BasicProperties>(message.Channel, string.Empty, true, props, data, cancellationToken: cancellationToken);
                result = new TransmissionResult(message.ID);
            }
            catch (Exception e)
            {
                result = new TransmissionResult(message.ID, Error: new(e, e switch
                {
                    PublishException => true,
                    _ => false
                }));
            }
            semaphore.Release();
            return result;
        }

        async ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        => await ProduceSubscriptionAsync(conn, channel, group,
                async (@event, model, acknowledge) =>
                {
                    var result = await messageReceived(ConvertMessage(@event, channel, acknowledge, out var messageID));
                    (var props, var data) = ConvertMessage(result, messageID);
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await this.channel.BasicPublishAsync<BasicProperties>(InboxExchange, @event.BasicProperties.ReplyTo!, true, props, data);
                    }
                    catch (Exception e)
                    {
                        errorReceived(e);
                    }
                    semaphore.Release();
                },
                errorReceived
            );

        ValueTask IMessageServiceConnection.CloseAsync()
         => ((IAsyncDisposable)this).DisposeAsync();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await semaphore.WaitAsync();
                await channel.CloseAsync();
                await channel.DisposeAsync();
                await conn.CloseAsync();
                await conn.DisposeAsync();
                semaphore.Release();
                semaphore.Dispose();
            }
        }
    }
}
