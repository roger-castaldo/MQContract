using Confluent.Kafka;
using MQContract.Interfaces.Service;
using MQContract.Kafka.Subscriptions;
using MQContract.Messages;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MQContract.Kafka
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using Kafka
    /// </summary>
    public sealed class Connection : IPingableMessageServiceConnection, IAsyncDisposable
    {
        private readonly record struct MessageInstance(string ID, string Channel, Message<string, byte[]> Message);

        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private bool disposedValue;

        private readonly ClientConfig clientConfig;
        private readonly IProducer<string, byte[]> producer;
        private readonly BatchedMessageStream<MessageInstance> batchedMessageStream;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="clientConfig">The Kafka Client Configuration to provide</param>
        public Connection(ClientConfig clientConfig)
        {
            this.clientConfig = clientConfig;
            var produceConfig = new ProducerConfig(CloneConfig(clientConfig))
            {
                // Reliability
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 5,
                RetryBackoffMs = 100,

                // Throughput
                LingerMs = 10,
                BatchSize = 131072,
                CompressionType = CompressionType.Lz4,

                // Ordering / Parallelism
                MaxInFlight = 5
            };
            producer = new ProducerBuilder<string, byte[]>(produceConfig)
                .Build();
            batchedMessageStream = new(
                (serviceMessage, _) => ValueTask.FromResult(new MessageInstance(
                    serviceMessage.ID,
                    serviceMessage.Channel,
                    new Message<string, byte[]>()
                    {
                        Key=serviceMessage.ID,
                        Headers=ExtractHeaders(serviceMessage),
                        Value=serviceMessage.Data.ToArray()
                    }
                )),
                (messageInstance, cancellationToken) =>
                {
                    try
                    {
                        var resultSource = new TaskCompletionSource<TransmissionResult>();
                        producer.Produce(messageInstance.Channel, messageInstance.Message,
                        (result) =>
                        {
                            if (!Equals(result.Status, PersistenceStatus.Persisted))
                                resultSource.TrySetResult(new(messageInstance.ID, Error: new(new PersistenceFailedException(), false)));
                            else
                                resultSource.TrySetResult(new TransmissionResult(result.Key));
                        });
                        return resultSource.Task;
                    }
                    catch (Exception ex)
                    {
                        return Task.FromResult<TransmissionResult>(
                            new(messageInstance.ID, Error: new(ex, ex switch
                            {
                                ProduceException<string, byte[]> => ((ProduceException<string, byte[]>)ex).Error.IsFatal,
                                _ => false
                            }))
                        );
                    }
                }
            );
        }

        /// <summary>
        /// Houses the supplied client configuration
        /// </summary>
        public ClientConfig ClientConfig => clientConfig;

        uint? IMessageServiceConnection.MaxMessageBodySize => (uint)Math.Abs(clientConfig.MessageMaxBytes??(1024*1024));

        internal static byte[] EncodeHeaderValue(string value)
            => UTF8Encoding.UTF8.GetBytes(value);

        internal static string DecodeHeaderValue(byte[] value)
            => UTF8Encoding.UTF8.GetString(value);

        internal static Headers ExtractHeaders(ServiceMessage message)
        {
            var result = new Headers();
            message.Header.ForEach(kv => result.Add(kv.Key, EncodeHeaderValue(kv.Value)));
            result.Add(MESSAGE_TYPE_HEADER, EncodeHeaderValue(message.MessageTypeID));
            return result;
        }

        internal static MessageHeader ExtractHeaders(Headers header, out string? messageTypeID)
        {
            if (header.TryGetLastBytes(MESSAGE_TYPE_HEADER, out var lastHeader))
                messageTypeID = DecodeHeaderValue(lastHeader);
            else
                messageTypeID=null;
            return new(
                header
                .Where(h => !Equals(h.Key, MESSAGE_TYPE_HEADER))
                .Select(h => new KeyValuePair<string, string?>(h.Key, DecodeHeaderValue(h.GetValueBytes())))
            );
        }

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(message, cancellationToken);

        ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(messages, cancellationToken);

        private static readonly Regex regReplyGroup = new(@"^reply-[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$", RegexOptions.Compiled|RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var isReply = regReplyGroup.IsMatch(group??string.Empty);
            var builder = new ConsumerBuilder<string, byte[]>(new ConsumerConfig(CloneConfig(clientConfig))
            {
                GroupId=(!string.IsNullOrWhiteSpace(group) ? group : Guid.NewGuid().ToString()),
                AutoOffsetReset = (isReply ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest),
                EnableAutoOffsetStore = false,
                EnableAutoCommit = false,
                // responsiveness tuning
                FetchMinBytes = 1,               // don't wait for larger batches on the broker
                FetchWaitMaxMs = 50,            // wait at most 50ms for FetchMinBytes to be satisfied
                MaxPartitionFetchBytes = clientConfig.MessageMaxBytes ?? (1024 * 1024), // limit per-partition fetch size
                QueuedMinMessages = 1,          // start delivering to the application with fewer queued messages
                AutoCommitIntervalMs = 1000,    // commit offsets to broker more frequently (still relying on StoreOffset)
                SocketKeepaliveEnable = true
            });
            if (isReply)
                builder.SetPartitionsAssignedHandler((c, partitions) =>
                    [.. partitions.Select(partition =>
                    {
                        var watermark = c.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(5));
                        return new TopicPartitionOffset(partition, ((watermark.High-watermark.Low) >= 1 ? new Offset(watermark.High-1) : Offset.Beginning));
                    })]
                );
            var consumer = builder.Build();
            consumer.Subscribe(channel);
            var subscription = new PublishSubscription(
                consumer,
                messageReceived,
                errorReceived,
                channel);
            subscription.Start();
            return ValueTask.FromResult<IServiceSubscription?>(subscription);
        }

        private static ClientConfig CloneConfig(ClientConfig clientConfig)
            => new(clientConfig.ToDictionary());

        ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            using var adminClient = new AdminClientBuilder(clientConfig).Build();
            var start = Stopwatch.GetTimestamp();
            var metaData = adminClient.GetMetadata(TimeSpan.FromMinutes(1));
            if (metaData.Brokers.Count>0)
                return ValueTask.FromResult<PingResult>(new(metaData.OriginatingBrokerName, string.Empty, Stopwatch.GetElapsedTime(start)));
            throw new UnableToPingException();
        }

        async ValueTask IMessageServiceConnection.CloseAsync()
            => await ((IAsyncDisposable)this).DisposeAsync();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await batchedMessageStream.DisposeAsync().ConfigureAwait(true);
                producer.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
