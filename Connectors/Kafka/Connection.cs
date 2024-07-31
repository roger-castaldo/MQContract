using Confluent.Kafka;
using Microsoft.Extensions.Caching.Memory;
using MQContract.Interfaces.Service;
using MQContract.Kafka.Options;
using MQContract.Kafka.Subscriptions;
using MQContract.Messages;
using MQContract.NATS.Subscriptions;
using System.Collections.Concurrent;
using System.Text;

namespace MQContract.Kafka
{
    public class Connection(ClientConfig clientConfig) : IMessageServiceConnection,IDisposable
    {
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private const string QUERY_IDENTIFIER_HEADER = "_QueryClientID";
        private const string REPLY_ID = "_QueryReplyID";
        private const string REPLY_CHANNEL_HEADER = "_QueryReplyChannel";
        private const string ERROR_MESSAGE_TYPE_ID = "KafkaQueryError";

        private readonly IProducer<string, byte[]> producer = new ProducerBuilder<string, byte[]>(clientConfig).Build();
        private readonly ClientConfig clientConfig = clientConfig;
        private readonly List<SubscriptionBase> subscriptions = [];
        private readonly SemaphoreSlim dataLock = new(1, 1);
        private readonly Guid Identifier = Guid.NewGuid();
        private readonly ConcurrentDictionary<Guid, ManualResetEventSlim> waitingCalls = new();
        private readonly IMemoryCache queryResponseCache = new MemoryCache(new MemoryCacheOptions());
        private bool disposedValue;

        public int? MaxMessageBodySize => clientConfig.MessageMaxBytes;

        public TimeSpan DefaultTimout { get; init; } = TimeSpan.FromMinutes(20);

        internal static byte[] EncodeHeaderValue(string value)
            => UTF8Encoding.UTF8.GetBytes(value);

        internal static string DecodeHeaderValue(byte[] value)
            => UTF8Encoding.UTF8.GetString(value);

        internal static Headers ExtractHeaders(ServiceMessage message, Guid? queryClientID = null, Guid? replyID = null,string? replyChannel=null)
        {
            var result = new Headers();
            foreach (var key in message.Header.Keys)
                result.Add(key, EncodeHeaderValue(message.Header[key]!));
            result.Add(MESSAGE_TYPE_HEADER, EncodeHeaderValue(message.MessageTypeID));
            if (queryClientID!=null)
                result.Add(QUERY_IDENTIFIER_HEADER, queryClientID.Value.ToByteArray());
            if (replyID!=null)
                result.Add(REPLY_ID,replyID.Value.ToByteArray());
            if (replyChannel!=null)
                result.Add(REPLY_CHANNEL_HEADER,EncodeHeaderValue(replyChannel!));
            return result;
        }

        private static MessageHeader ExtractHeaders(Headers header)
            => new MessageHeader(
                header
                .Where(h => 
                    !Equals(h.Key, REPLY_ID)
                    &&!Equals(h.Key, REPLY_CHANNEL_HEADER)
                    &&!Equals(h.Key, MESSAGE_TYPE_HEADER)
                    &&!Equals(h.Key,QUERY_IDENTIFIER_HEADER)
                )
                .Select(h => new KeyValuePair<string, string>(h.Key, DecodeHeaderValue(h.GetValueBytes())))
            );

        internal static MessageHeader ExtractHeaders(Headers header,out string? messageTypeID)
        {
            messageTypeID = DecodeHeaderValue(header.FirstOrDefault(pair => Equals(pair.Key, MESSAGE_TYPE_HEADER))?.GetValueBytes()?? []);
            return ExtractHeaders(header);
        }

        internal static MessageHeader ExtractHeaders(Headers header, out string? messageTypeID,out Guid? queryClient,out Guid? replyID,out string? replyChannel)
        {
            messageTypeID = DecodeHeaderValue(header.FirstOrDefault(pair => Equals(pair.Key, MESSAGE_TYPE_HEADER))?.GetValueBytes()?? []);
            queryClient = new Guid(header.FirstOrDefault(pair => Equals(pair.Key, QUERY_IDENTIFIER_HEADER))?.GetValueBytes()?? Guid.Empty.ToByteArray());
            replyID = new Guid(header.FirstOrDefault(pair => Equals(pair.Key, REPLY_ID))?.GetValueBytes()?? Guid.Empty.ToByteArray());
            replyChannel = DecodeHeaderValue(header.FirstOrDefault(pair => Equals(pair.Key, REPLY_CHANNEL_HEADER))?.GetValueBytes()?? []);
            return ExtractHeaders(header);
        }

        public Task<PingResult> PingAsync()
            => throw new NotImplementedException();

        public async Task<TransmissionResult> PublishAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await producer.ProduceAsync(message.Channel, new Message<string, byte[]>()
                {
                    Key=message.ID,
                    Headers=ExtractHeaders(message),
                    Value=message.Data.ToArray()
                });
                return new TransmissionResult(result.Key);
            }
            catch (Exception ex)
            {
                return new TransmissionResult(message.ID, ex.Message);
            }
        }

        public async Task<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options is not QueryChannelOptions queryChannelOptions)
                throw new ArgumentNullException(nameof(options));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(queryChannelOptions.ReplyChannel, nameof(queryChannelOptions.ReplyChannel));
            using var queryLock = new ManualResetEventSlim(false);
            var callID = Guid.NewGuid();
            var headers = ExtractHeaders(message, Identifier, callID, queryChannelOptions.ReplyChannel);
            if (!waitingCalls.TryAdd(callID, queryLock))
                throw new QueryLockFailedException();
            try
            {
                StartResponseListener(callID,queryChannelOptions.ReplyChannel,cancellationToken,queryLock);
                queryLock.Wait();
                queryLock.Reset();
                await producer.ProduceAsync(message.Channel, new Message<string, byte[]>()
                {
                    Key=message.ID,
                    Headers=headers,
                    Value=message.Data.ToArray()
                },cancellationToken);
            }catch(Exception)
            {
                waitingCalls.Remove(callID, out _);
                throw;
            }
            if (queryLock.Wait(timeout,cancellationToken))
            {
                waitingCalls.Remove(callID, out _);
                if (queryResponseCache.TryGetValue<ServiceQueryResult>(callID, out ServiceQueryResult? result))
                {
                    if (Equals(result?.MessageTypeID, ERROR_MESSAGE_TYPE_ID))
                        throw new QueryAsyncReponseException(DecodeHeaderValue(result.Data.ToArray()));
                    return result!;
                }
                throw new QueryResultMissingException();
            }
            waitingCalls.Remove(callID,out _);
            throw new QueryExecutionFailedException();
        }

        private void StartResponseListener(Guid callID, string replyChannel,CancellationToken cancellationToken, ManualResetEventSlim queryLock)
        {
            var consumer = new ConsumerBuilder<string, byte[]>(new ConsumerConfig(clientConfig)
            {
                AutoOffsetReset=AutoOffsetReset.Latest
            }).Build();
            consumer.Subscribe(replyChannel);
            Task.Run(() =>
            {
                queryLock.Set();
                ServiceQueryResult? result = null;
                while (!cancellationToken.IsCancellationRequested && waitingCalls.TryGetValue(callID, out _))
                {
                    try
                    {
                        var msg = consumer.Consume(cancellationToken);
                        var headers = ExtractHeaders(msg.Message.Headers, out var messageTypeID, out var queryClient, out var replyID, out var replyChannel);
                        if (Equals(queryClient, Identifier) && Equals(replyID, callID))
                        {

                            result = new(
                                msg.Message.Key,
                                headers,
                                messageTypeID!,
                                msg.Message.Value
                            );
                            break;
                        }
                    }
                    catch (Exception) { }
                }
                try
                {
                    consumer.Close();
                    consumer.Dispose();
                }
                catch (Exception) { }
                if (result!=null)
                {
                    queryResponseCache.Set(callID, result, DateTimeOffset.Now.AddMinutes(10));
                    queryLock.Set();
                }
            },cancellationToken);
        }

        public async Task<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            var subscription = new PublishSubscription(
                new ConsumerBuilder<string,byte[]>(new ConsumerConfig(clientConfig)
                {
                    GroupId=(!string.IsNullOrWhiteSpace(group) ? group : null)
                }).Build(),
                messageRecieved,
                errorRecieved,
                channel,
                cancellationToken);
            subscription.Run();
            await dataLock.WaitAsync(cancellationToken);
            subscriptions.Add(subscription);
            dataLock.Release();
            return subscription;
        }

        public async Task<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, Task<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null && options is not QueryChannelOptions)
                throw new InvalidChannelOptionsTypeException(typeof(QueryChannelOptions), options.GetType());
            var subscription = new QuerySubscription(
                new ConsumerBuilder<string, byte[]>(new ConsumerConfig(clientConfig)
                {
                    GroupId=(!string.IsNullOrWhiteSpace(group) ? group : null)
                }).Build(),
                async (recievedMessage) =>
                {
                    var headers = ExtractHeaders(recievedMessage.Headers,out var messageTypeID,out var queryClient,out var replyID,out var replyChannel);
                    var recievedServiceMessage = new RecievedServiceMessage(
                        recievedMessage.Key,
                        messageTypeID??string.Empty,
                        channel,
                        headers,
                        recievedMessage.Value
                    );
                    try
                    {
                        var result = await messageRecieved(recievedServiceMessage);
                        await producer.ProduceAsync(
                            replyChannel??((QueryChannelOptions?)options)?.ReplyChannel??string.Empty,
                            new Message<string, byte[]>()
                            {
                                Key=result.ID,
                                Headers=ExtractHeaders(result,queryClient,replyID,replyChannel),
                                Value=result.Data.ToArray()
                            }
                        );
                    }
                    catch(Exception e)
                    {
                        var respMessage = new ServiceMessage(recievedMessage.Key, ERROR_MESSAGE_TYPE_ID,replyChannel??string.Empty, new MessageHeader([]), EncodeHeaderValue(e.Message));
                        await producer.ProduceAsync(
                            replyChannel??((QueryChannelOptions?)options)?.ReplyChannel??string.Empty,
                            new Message<string, byte[]>()
                            {
                                Key=recievedMessage.Key,
                                Headers=ExtractHeaders(respMessage,queryClient,replyID,replyChannel),
                                Value=respMessage.Data.ToArray()
                            }
                        );
                    }
                },
                errorRecieved,
                channel,
                cancellationToken);
            subscription.Run();
            await dataLock.WaitAsync(cancellationToken);
            subscriptions.Add(subscription);
            dataLock.Release();
            return subscription;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dataLock.Wait();
                    foreach (var sub in subscriptions)
                        sub.EndAsync().Wait();
                    subscriptions.Clear();
                    producer.Dispose();
                    dataLock.Release();
                    dataLock.Dispose();
                }
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
