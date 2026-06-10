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
    public sealed class Connection : IPingableMessageServiceConnection, IAsyncDisposable
    {
        private readonly record struct MessageInstance(string ID, string Channel, MessageMetadata MessageMetadata, ReadOnlyMemory<byte> Data);
        private const string MessageTypeID = "x-mqcontract-message-type";

        private readonly ConcurrentDictionary<string, IProducer<byte[]>> producers = new();
        private readonly BatchedMessageStream<MessageInstance> batchedMessageStream;
        private bool disposed;

        /// <summary>
        /// The underlying connection, exposed for external usage
        /// </summary>
        public IPulsarClient PulsarClient { get; private init; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="pulsarClientBuilder">An instance of a pulsar client builder used to build the underlying client connection</param>
        public Connection(IPulsarClientBuilder pulsarClientBuilder)
        {
            PulsarClient = pulsarClientBuilder.Build();
            batchedMessageStream = new(
                async (serviceMessage, _) =>
                {
                    var messageMetadata = new MessageMetadata()
                    {
                        Key = serviceMessage.ID
                    };
                    messageMetadata[MessageTypeID] = serviceMessage.MessageTypeID;
                    serviceMessage.Header.ForEach(pair =>
                    {
                         messageMetadata[pair.Key] = pair.Value;
                    });
                    return new MessageInstance(serviceMessage.ID, serviceMessage.Channel, messageMetadata, serviceMessage.Data);
                },
                async (messageInstance, cancellationToken) =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return new TransmissionResult(messageInstance.ID, Error: new(new OperationCanceledException("Transmission cancelled"), true));
                        if (!producers.TryGetValue(messageInstance.Channel, out var producer))
                        {
                            producer = PulsarClient.CreateProducer<byte[]>(new(messageInstance.Channel, Schema.ByteArray));
                            producers.TryAdd(messageInstance.Channel, producer);
                        }
                        _ = await producer.Send(messageInstance.MessageMetadata, messageInstance.Data.ToArray(), cancellationToken);
                        return new TransmissionResult(messageInstance.ID);
                    }
                    catch (Exception ex)
                    {
                        return new TransmissionResult(messageInstance.ID, Error: new(ex, ex switch
                        {
                            ProducerFaultedException => true,
                            ProducerClosedException => true,
                            ProducerDisposedException => true,
                            ProducerFencedException => false,
                            _ => false
                        }));
                    }
                });
        }

        /// <summary>
        /// Max Message Body Size in bytes, default 5MB
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 5*1024*1024;

        async ValueTask IMessageServiceConnection.CloseAsync()
            => await batchedMessageStream.DisposeAsync();

        internal static ReceivedServiceMessage ConvertMessage(IMessage<byte[]> message, string channel, Func<ValueTask> acknowledge)
            => new(
                message.Key!,
                message.Properties[MessageTypeID],
                channel,
                new(message.Properties.Where(pair => !Equals(pair.Key, MessageTypeID)).Select(pair=>new KeyValuePair<string,string?>(pair.Key,pair.Value))),
                message.Data.ToArray(),
                acknowledge
            );

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(message, cancellationToken);
        ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(messages, cancellationToken);

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
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
                await using var producer = PulsarClient.CreateProducer<byte[]>(new("non-persistent://public/default/heartbeat", Schema.ByteArray)
                {
                    MaxPendingMessages=1
                });
                _ = await producer.Send(new(), new byte[0]); // empty payload
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
                await batchedMessageStream.DisposeAsync();
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
