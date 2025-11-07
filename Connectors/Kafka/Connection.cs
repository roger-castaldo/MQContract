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

        internal static MessageHeader ExtractHeaders(Headers header, out string? messageTypeID)
        {
            messageTypeID = DecodeHeaderValue(header.FirstOrDefault(pair => Equals(pair.Key, MESSAGE_TYPE_HEADER))?.GetValueBytes()?? []);
            return ExtractHeaders(header);
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var result = await producer.ProduceAsync(message.Channel, new Message<string, byte[]>()
                {
                    Key=message.ID,
                    Headers=ExtractHeaders(message),
                    Value=message.Data.ToArray()
                }, cancellationToken);
                if (!Equals(result.Status, PersistenceStatus.Persisted))
                    return new(message.ID, Error: new(new PersistenceFailedException(), false));
                return new TransmissionResult(result.Key);
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

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var isReply = regReplyGroup.IsMatch(group??string.Empty);
            var builder = new ConsumerBuilder<string, byte[]>(new ConsumerConfig(clientConfig)
            {
                GroupId=(!string.IsNullOrWhiteSpace(group) ? group : Guid.NewGuid().ToString()),
                AutoOffsetReset = (isReply ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest)
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
