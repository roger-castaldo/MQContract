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
    /// <param name="clientConfig">The Kafka Client Configuration to provide</param>
    public sealed class Connection(ClientConfig clientConfig) : IPingableMessageServiceConnection
    {
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";

        private readonly IProducer<string, byte[]> producer = new ProducerBuilder<string, byte[]>(clientConfig).Build();

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
            Activity.Current?.AddEvent(new("Converting Kafka Headers"));
            var result = new Headers();
            foreach (var key in message.Header.Keys)
                result.Add(key, EncodeHeaderValue(message.Header[key]!));
            result.Add(MESSAGE_TYPE_HEADER, EncodeHeaderValue(message.MessageTypeID));
            Activity.Current?.AddEvent(new("Headers converted"));
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
                .Select(h => new KeyValuePair<string, string>(h.Key, DecodeHeaderValue(h.GetValueBytes())))
            );
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            Activity.Current?.AddEvent(new("Publishing message through Kafka"));
            try
            {
                Activity.Current?.AddEvent(new("Producing Message"));
                var resultSource = new TaskCompletionSource<TransmissionResult>();
                producer.Produce(message.Channel, new Message<string, byte[]>()
                {
                    Key=message.ID,
                    Headers=ExtractHeaders(message),
                    Value=message.Data.ToArray()
                }, 
                (result) =>
                {
                    if (!Equals(result.Status, PersistenceStatus.Persisted))
                        resultSource.TrySetResult(new(message.ID, Error: new(new PersistenceFailedException(), false)));
                    else
                        resultSource.TrySetResult(new TransmissionResult(result.Key));
                });
                var result = await resultSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                Activity.Current?.AddEvent(new("Returning transmission result"));
                return result;
            }
            catch (Exception ex)
            {
                return new TransmissionResult(message.ID, Error: new(ex, ex switch
                {
                    ProduceException<string, byte[]> => ((ProduceException<string, byte[]>)ex).Error.IsFatal,
                    _ => false
                }));
            }
        }

        private static readonly Regex regReplyGroup = new Regex(@"^reply-[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$", RegexOptions.Compiled|RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var isReply = regReplyGroup.IsMatch(group??string.Empty);
            var builder = new ConsumerBuilder<string, byte[]>(new ConsumerConfig(clientConfig)
            {
                GroupId=(!string.IsNullOrWhiteSpace(group) ? group : Guid.NewGuid().ToString()),
                AutoOffsetReset = (isReply ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest),
                EnableAutoOffsetStore = false,
                EnableAutoCommit = false
            });
            if (isReply)
                builder.SetPartitionsAssignedHandler((c, partitions) =>
                    partitions.Select(partition =>
                    {
                        var watermark = c.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(5));
                        return new TopicPartitionOffset(partition, ((watermark.High-watermark.Low) >= 1 ? new Offset(watermark.High-1) : Offset.Beginning));
                    })
                    .ToArray()
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

        ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            using var adminClient = new AdminClientBuilder(clientConfig).Build();
            var start = Stopwatch.GetTimestamp();
            var metaData = adminClient.GetMetadata(TimeSpan.FromMinutes(1));
            if (metaData.Brokers.Count>0)
                return ValueTask.FromResult<PingResult>(new(metaData.OriginatingBrokerName,string.Empty,Stopwatch.GetElapsedTime(start)));
            throw new UnableToPingException();
        }

        ValueTask IMessageServiceConnection.CloseAsync()
        {
            producer.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
