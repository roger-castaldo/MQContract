using Confluent.Kafka;
using MQContract.Interfaces.Service;
using MQContract.Kafka.Subscriptions;
using MQContract.Messages;
using System.Text;

namespace MQContract.Kafka
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using Kafka
    /// </summary>
    /// <param name="clientConfig">The Kafka Client Configuration to provide</param>
    public sealed class Connection(ClientConfig clientConfig) : IMessageServiceConnection
    {
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";

        private readonly IProducer<string, byte[]> producer = new ProducerBuilder<string, byte[]>(clientConfig).Build();
        private readonly ClientConfig clientConfig = clientConfig;
        private bool disposedValue;

        /// <summary>
        /// The maximum message body size allowed
        /// </summary>
        public uint? MaxMessageBodySize => (uint)Math.Abs(clientConfig.MessageMaxBytes??(1024*1024));

        internal static byte[] EncodeHeaderValue(string value)
            => UTF8Encoding.UTF8.GetBytes(value);

        internal static string DecodeHeaderValue(byte[] value)
            => UTF8Encoding.UTF8.GetString(value);

        internal static Headers ExtractHeaders(ServiceMessage message)
        {
            var result = new Headers();
            foreach (var key in message.Header.Keys)
                result.Add(key, EncodeHeaderValue(message.Header[key]!));
            result.Add(MESSAGE_TYPE_HEADER, EncodeHeaderValue(message.MessageTypeID));
            return result;
        }

        private static MessageHeader ExtractHeaders(Headers header)
            => new(
                header
                .Where(h => !Equals(h.Key, MESSAGE_TYPE_HEADER))
                .Select(h => new KeyValuePair<string, string>(h.Key, DecodeHeaderValue(h.GetValueBytes())))
            );

        internal static MessageHeader ExtractHeaders(Headers header,out string? messageTypeID)
        {
            messageTypeID = DecodeHeaderValue(header.FirstOrDefault(pair => Equals(pair.Key, MESSAGE_TYPE_HEADER))?.GetValueBytes()?? []);
            return ExtractHeaders(header);
        }

        /// <summary>
        /// Called to publish a message into the Kafka server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Transmition result identifying if it worked or not</returns>
        public async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await producer.ProduceAsync(message.Channel, new Message<string, byte[]>()
                {
                    Key=message.ID,
                    Headers=ExtractHeaders(message),
                    Value=message.Data.ToArray()
                },cancellationToken);
                return new TransmissionResult(result.Key);
            }
            catch (Exception ex)
            {
                return new TransmissionResult(message.ID, ex.Message);
            }
        }

        /// <summary>
        /// Called to create a subscription to the underlying Kafka server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a message is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The name of the group to bind the consumer to</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        public async ValueTask<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string? group = null, CancellationToken cancellationToken = default)
        {
            var subscription = new PublishSubscription(
                new ConsumerBuilder<string,byte[]>(new ConsumerConfig(clientConfig)
                {
                    GroupId=(!string.IsNullOrWhiteSpace(group) ? group : Guid.NewGuid().ToString())
                }).Build(),
                messageRecieved,
                errorRecieved,
                channel);
            await subscription.Run();
            return subscription;
        }

        /// <summary>
        /// Called to close off the underlying Kafka Connection
        /// </summary>
        /// <returns></returns>
        public ValueTask CloseAsync()
        {
            Dispose(true);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Called to dispose of the object correctly and allow it to clean up it's resources
        /// </summary>
        /// <returns>A task required for disposal</returns>
        public ValueTask DisposeAsync()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    producer.Dispose();
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
