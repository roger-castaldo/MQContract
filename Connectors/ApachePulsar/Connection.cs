using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Exceptions;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Buffers;

namespace MQContract.ApachePulsar
{
    /// <summary>
    /// This is the MessageServiceConnection implemenation for using ApaxhePulsar
    /// </summary>
    /// <param name="pulsarClientBuilder">An instance of a pulsar client builder used to build the underlying client connection</param>
    public class Connection(IPulsarClientBuilder pulsarClientBuilder) : IMessageServiceConnection, IAsyncDisposable
    {
        private const string MessageTypeID = "_MessageTypeID";

        private readonly IPulsarClient pulsarClient = pulsarClientBuilder.Build();
        private readonly SemaphoreSlim producerLock = new(1, 1);
        private readonly Dictionary<string, IProducer<byte[]>> producers = new();
        private bool disposed;


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
            await producerLock.WaitAsync();
            if (!producers.TryGetValue(message.Channel, out var producer))
            {
                producer = pulsarClient.CreateProducer<byte[]>(new(message.Channel, Schema.ByteArray));
                producers.Add(message.Channel, producer);
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
            finally
            {
                producerLock.Release();
            }
        }

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var subscription = new Subscription(
                pulsarClient,
                messageReceived,
                errorReceived,
                channel,
                group
            );
            subscription.Start();
            return ValueTask.FromResult<IServiceSubscription?>(subscription);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposed)
            {
                disposed=true;
                await producerLock.WaitAsync();
                foreach (var p in producers.Values)
                    await p.DisposeAsync();
                producers.Clear();
                await pulsarClient.DisposeAsync();
                producerLock.Release();
                producerLock.Dispose();
            }
        }
    }
}
