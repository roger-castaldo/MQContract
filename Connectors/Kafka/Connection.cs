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
        public int? MaxMessageBodySize => clientConfig.MessageMaxBytes;

        /// <summary>
        /// The default timeout to use for RPC calls when not specified by the class or in the call.
        /// DEFAULT:1 minute if not specified inside the connection options
        /// </summary>
        public TimeSpan DefaultTimout { get; init; } = TimeSpan.FromMinutes(1);

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
        /// <param name="options">The service channel options which should be null as there is no implementations for Kafka</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Transmition result identifying if it worked or not</returns>
        /// <exception cref="NoChannelOptionsAvailableException">Thrown if options was supplied because there are no implemented options for this call</exception>
        public async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            NoChannelOptionsAvailableException.ThrowIfNotNull(options);
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
        /// <param name="group">The group to subscribe as part of</param>
        /// <param name="options">should be null</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NoChannelOptionsAvailableException">Thrown if options was supplied because there are no implemented options for this call</exception>
        public async ValueTask<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            NoChannelOptionsAvailableException.ThrowIfNotNull(options);
            var subscription = new PublishSubscription(
                new ConsumerBuilder<string,byte[]>(new ConsumerConfig(clientConfig)
                {
                    GroupId=(!string.IsNullOrWhiteSpace(group) ? group : null)
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
