using Confluent.Kafka;
using MQContract.Interfaces.Service;
using MQContract.Kafka.Options;
using MQContract.Kafka.Subscriptions;
using MQContract.Messages;
using MQContract.NATS.Subscriptions;
using System.Text;

namespace MQContract.Kafka
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using Kafka
    /// </summary>
    /// <param name="clientConfig"></param>
    public class Connection(ClientConfig clientConfig) : IMessageServiceConnection
    {
        private const string MESSAGE_TYPE_HEADER = "_MessageTypeID";
        private const string QUERY_IDENTIFIER_HEADER = "_QueryClientID";
        private const string REPLY_ID = "_QueryReplyID";
        private const string REPLY_CHANNEL_HEADER = "_QueryReplyChannel";
        private const string ERROR_MESSAGE_TYPE_ID = "KafkaQueryError";

        private readonly IProducer<string, byte[]> producer = new ProducerBuilder<string, byte[]>(clientConfig).Build();
        private readonly ClientConfig clientConfig = clientConfig;
        private readonly Guid Identifier = Guid.NewGuid();
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

        /// <summary>
        /// Not implemented as Kafka does not support this particular action
        /// </summary>
        /// <returns>Throws NotImplementedException</returns>
        /// <exception cref="NotImplementedException">Thrown because Kafka does not support this particular action</exception>
        public Task<PingResult> PingAsync()
            => throw new NotImplementedException();

        /// <summary>
        /// Called to publish a message into the Kafka server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="options">The service channel options, if desired, specifically the PublishChannelOptions which is used to access the storage capabilities of KubeMQ</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Transmition result identifying if it worked or not</returns>
        /// <exception cref="NoChannelOptionsAvailableException">Thrown if options was supplied because there are no implemented options for this call</exception>
        public async Task<TransmissionResult> PublishAsync(ServiceMessage message, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
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
        /// Called to publish a query into the Kafka server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="timeout">The timeout supplied for the query to response</param>
        /// <param name="options">The options specifically for this call and must be supplied.  Must be instance of QueryChannelOptions.</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The resulting response</returns>
        /// <exception cref="ArgumentNullException">Thrown if options is null</exception>
        /// <exception cref="InvalidChannelOptionsTypeException">Thrown if the options that was supplied is not an instance of QueryChannelOptions</exception>
        /// <exception cref="ArgumentNullException">Thrown if the ReplyChannel is blank or null as it needs to be set</exception>
        /// <exception cref="QueryExecutionFailedException">Thrown when the query fails to execute</exception>
        /// <exception cref="QueryAsyncReponseException">Thrown when the responding instance has provided an error</exception>
        /// <exception cref="QueryResultMissingException">Thrown when there is no response to be found for the query</exception>
        public async Task<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);
            InvalidChannelOptionsTypeException.ThrowIfNotNullAndNotOfType<QueryChannelOptions>(options);
            var queryChannelOptions = (QueryChannelOptions)options;
            ArgumentNullException.ThrowIfNullOrWhiteSpace(queryChannelOptions.ReplyChannel);
            var callID = Guid.NewGuid();
            var headers = ExtractHeaders(message, Identifier, callID, queryChannelOptions.ReplyChannel);
            var tcs = StartResponseListener(clientConfig,Identifier,callID,queryChannelOptions.ReplyChannel,cancellationToken);
            await producer.ProduceAsync(message.Channel, new Message<string, byte[]>()
            {
                Key=message.ID,
                Headers=headers,
                Value=message.Data.ToArray()
            },cancellationToken);
            try
            {
                await tcs.Task.WaitAsync(timeout, cancellationToken);
            }
            catch (Exception)
            {
                throw new QueryExecutionFailedException();
            }
            if (tcs.Task.IsCompleted)
            {
                var result = tcs.Task.Result;
                if (Equals(result?.MessageTypeID, ERROR_MESSAGE_TYPE_ID))
                    throw new QueryAsyncReponseException(DecodeHeaderValue(result.Data.ToArray()));
                else if (result!=null)
                    return result;
            }
            throw new QueryResultMissingException();
        }

        private static TaskCompletionSource<ServiceQueryResult> StartResponseListener(ClientConfig configuration,Guid identifier,Guid callID, string replyChannel,CancellationToken cancellationToken)
        {
            var result = new TaskCompletionSource<ServiceQueryResult>();
            using var queryLock = new ManualResetEventSlim(false);
            Task.Run(() =>
            {
                using var consumer = new ConsumerBuilder<string, byte[]>(new ConsumerConfig(configuration)
                {
                    AutoOffsetReset=AutoOffsetReset.Earliest
                }).Build();
                consumer.Subscribe(replyChannel);
                queryLock.Set();
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var msg = consumer.Consume(cancellationToken);
                        var headers = ExtractHeaders(msg.Message.Headers, out var messageTypeID, out var queryClient, out var replyID, out var replyChannel);
                        if (Equals(queryClient, identifier) && Equals(replyID, callID))
                        {
                            Console.WriteLine(result.TrySetResult(new(
                                msg.Message.Key,
                                headers,
                                messageTypeID!,
                                msg.Message.Value
                            )));
                            consumer.Unassign();
                            break;
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                }
                try
                {
                    consumer.Close();
                }
                catch (Exception) { }
            },cancellationToken);
            queryLock.Wait(cancellationToken);
            return result;
        }

        /// <summary>
        /// Called to create a subscription to the underlying Kafka server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a message is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The group to subscribe as part of</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NoChannelOptionsAvailableException">Thrown if options was supplied because there are no implemented options for this call</exception>
        public async Task<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            NoChannelOptionsAvailableException.ThrowIfNotNull(options);
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
            return subscription;
        }

        /// <summary>
        /// Called to create a subscription for queries to the underlying Kafka server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a query is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The group to subscribe as part of</param>
        /// <param name="options">Optional QueryChannelOptions to be supplied that will specify the ReplyChannel if not supplied by query message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance</returns>
        /// <exception cref="InvalidChannelOptionsTypeException">Thrown when options is not null and is not an instance of the type QueryChannelOptions</exception>
        public async Task<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, Task<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            InvalidChannelOptionsTypeException.ThrowIfNotNullAndNotOfType<QueryChannelOptions>(options);
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
            return subscription;
        }

        public ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                producer.Dispose();
            }
            return ValueTask.CompletedTask;
        }
    }
}
