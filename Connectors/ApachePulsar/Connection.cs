using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Exceptions;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MQContract.ApachePulsar
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using ApaxhePulsar
    /// </summary>
    /// <param name="pulsarClientBuilder">An instance of a pulsar client builder used to build the underlying client connection</param>
    public sealed class Connection(IPulsarClientBuilder pulsarClientBuilder) : IPingableMessageServiceConnection, IAsyncDisposable
    {
        private const string MessageTypeID = "_MessageTypeID";

        private readonly ConcurrentDictionary<string, IProducer<byte[]>> producers = new();
        private bool disposed;

        /// <summary>
        /// The underlying connection, exposed for external usage
        /// </summary>
        public IPulsarClient PulsarClient { get; private init; } = pulsarClientBuilder.Build();


        /// <summary>
        /// Max Message Body Size in bytes, default 5MB
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 5*1024*1024;

        ValueTask IMessageServiceConnection.CloseAsync()
            => ValueTask.CompletedTask;

        private static (MessageMetadata messageMetadata, byte[] data) Convert(ServiceMessage message)
        {
            var messageMetadata = new MessageMetadata()
            {
                Key = message.ID
            };
            messageMetadata[MessageTypeID] = message.MessageTypeID;
            foreach (var key in message.Header.Keys)
                messageMetadata[key] = message.Header[key];
            return (messageMetadata, message.Data.ToArray());
        }

        internal static ReceivedServiceMessage ConvertMessage(IMessage<byte[]> message, string channel, Func<ValueTask> acknowledge)
            => new(
                message.Key!,
                message.Properties[MessageTypeID],
                channel,
                new(message.Properties.Where(pair => !Equals(pair.Key, MessageTypeID))),
                message.Data.ToArray(),
                acknowledge
            );

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            if (!producers.TryGetValue(message.Channel,out var producer))
            {
                producer = PulsarClient.CreateProducer<byte[]>(new(message.Channel, Schema.ByteArray));
                producers.TryAdd(message.Channel, producer);
            }
            (var messageMetaData, var data) = Convert(message);
            try
            {
                _ = await producer.Send(messageMetaData, data, cancellationToken);
                return new(message.ID);
            }
            catch (Exception ex)
            {
                return new(message.ID, Error: new(ex, ex switch
                {
                    ProducerFaultedException => true,
                    ProducerClosedException => true,
                    ProducerDisposedException => true,
                    ProducerFencedException => false,
                    _ => false
                }));
            }
        }

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var subscription = new Subscription(
                PulsarClient,
                messageReceived,
                errorReceived,
                channel,
                group
            );
            subscription.Start();
            return ValueTask.FromResult<IServiceSubscription?>(subscription);
        }

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            try
            {
                var start = Stopwatch.GetTimestamp();
                await using var producer = PulsarClient.CreateProducer<byte[]>(new("non-persistent://public/default/heartbeat", Schema.ByteArray));
                _ = await producer.Send(new(),new byte[0]); // empty payload
                return new(PulsarClient.ServiceUrl.ToString(), string.Empty, Stopwatch.GetElapsedTime(start));
            }
            catch
            {
                throw new PingFailedException("Unable to create a producer and publish to the heartbeat path");
            }
            
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposed)
            {
                disposed=true;
                var keys = producers.Keys;
                foreach (var k in keys)
                {
                    if (producers.TryRemove(k, out var producer))
                        await producer.DisposeAsync();
                }
                producers.Clear();
                await PulsarClient.DisposeAsync();
            }
        }
    }
}
