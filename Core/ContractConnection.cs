using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Factories;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Subscriptions;
using System.Reflection;

namespace MQContract
{
    public class ContractConnection(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
                : IContractConnection
    {
        private readonly SemaphoreSlim dataLock = new(1, 1);
        private IEnumerable<IMessageTypeFactory> typeFactories = [];

        private IMessageFactory<T> GetMessageFactory<T>(bool ignoreMessageHeader = false) where T : class
        {
            dataLock.Wait();
            var result = (IMessageFactory<T>?)typeFactories.FirstOrDefault(fact => fact.GetType().GetGenericArguments()[0]==typeof(T));
            dataLock.Release();
            if (result==null)
            {
                result = new MessageTypeFactory<T>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, ignoreMessageHeader, serviceConnection.MaxMessageBodySize);
                dataLock.Wait();
                if (!typeFactories.Any(fact => fact.GetType().GetGenericArguments()[0]==typeof(T) && fact.IgnoreMessageHeader==ignoreMessageHeader))
                    typeFactories = typeFactories.Concat([(IMessageTypeFactory)result]);
                dataLock.Release();
            }
            return result;
        }

        private Task<string> MapChannel(ChannelMapper.MapTypes mapType, string originalChannel)
            => channelMapper?.MapChannel(mapType, originalChannel)??Task.FromResult<string>(originalChannel);

        public Task<PingResult> PingAsync()
            => serviceConnection.PingAsync();

        public async Task<TransmissionResult> PublishAsync<T>(T message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class
            => await serviceConnection.PublishAsync(
                await ProduceServiceMessage<T>(ChannelMapper.MapTypes.Publish,message, channel: channel, messageHeader: messageHeader),
                timeout??TimeSpan.FromMilliseconds(typeof(T).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.Value??serviceConnection.DefaultTimout.TotalMilliseconds),
                options,
                cancellationToken
            );

        private async Task<ServiceMessage> ProduceServiceMessage<T>(ChannelMapper.MapTypes mapType,T message, string? channel = null, IMessageHeader? messageHeader = null) where T : class
            => await GetMessageFactory<T>().ConvertMessageAsync(message, channel, messageHeader,(originalChannel)=>MapChannel(mapType,originalChannel));

        public async Task<ISubscription> SubscribeAsync<T>(Func<IMessage<T>,Task> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, bool synchronous = false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default) where T : class
        {
            var subscription = new PubSubSubscription<T>(GetMessageFactory<T>(ignoreMessageHeader),
                messageRecieved,
                errorRecieved,
                (originalChannel)=>MapChannel(ChannelMapper.MapTypes.PublishSubscription,originalChannel),
                channel:channel,
                group:group,
                synchronous:synchronous,
                options:options,
                logger:logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection,cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }

        private async Task<QueryResult<R>> ExecuteQueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class
            => ProduceResultAsync<R>(await serviceConnection.QueryAsync(
                await ProduceServiceMessage<Q>(ChannelMapper.MapTypes.Query,message, channel: channel, messageHeader: messageHeader),
                timeout??TimeSpan.FromMilliseconds(typeof(Q).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.Value??serviceConnection.DefaultTimout.TotalMilliseconds),
                options,
                cancellationToken
            ));

        public async Task<QueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class
            => await ExecuteQueryAsync<Q, R>(message, timeout: timeout, channel: channel, messageHeader: messageHeader, options: options, cancellationToken: cancellationToken);

        public async Task<QueryResult<object>> QueryAsync<Q>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null,
            IServiceChannelOptions? options = null, CancellationToken cancellationToken = default) where Q : class
        {
            var responseType = (typeof(Q).GetCustomAttribute<QueryResponseTypeAttribute>(false)?.ResponseType)??throw new UnknownResponseTypeException("ResponseType", typeof(Q));
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var methodInfo = typeof(ContractConnection).GetMethod(nameof(ContractConnection.ExecuteQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance)!.MakeGenericMethod(typeof(Q), responseType!);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var task = (Task)methodInfo.Invoke(this, [
                    message,
                    timeout,
                    channel,
                    messageHeader,
                    options,
                    cancellationToken
                ])!;
            await task.ConfigureAwait(false);
            var queryResult = ((dynamic)task).Result;
            return new QueryResult<object>(queryResult.ID, queryResult.Header, queryResult.Result, queryResult.Error);
        }

        private QueryResult<R> ProduceResultAsync<R>(ServiceQueryResult queryResult) where R : class
        {
            try
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: GetMessageFactory<R>().ConvertMessage(logger, queryResult)
                );
            }catch(QueryResponseException qre)
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: qre.Message
                );
            }
            catch(Exception ex)
            {
                return new(
                    queryResult.ID,
                    queryResult.Header,
                    Result: default,
                    Error: ex.Message
                );
            }
        }

        public async Task<ISubscription> SubscribeQueryResponseAsync<Q, R>(Func<IMessage<Q>, Task<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, bool ignoreMessageHeader = false, bool synchronous = false, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
            where Q : class
            where R : class
        {
            var subscription = new QueryResponseSubscription<Q,R>(
                GetMessageFactory<Q>(ignoreMessageHeader),
                GetMessageFactory<R>(),
                messageRecieved,
                errorRecieved,
                (originalChannel) => MapChannel(ChannelMapper.MapTypes.QuerySubscription, originalChannel),
                channel: channel,
                group: group,
                synchronous: synchronous,
                options: options,
                logger: logger);
            if (await subscription.EstablishSubscriptionAsync(serviceConnection, cancellationToken))
                return subscription;
            throw new SubscriptionFailedException();
        }
    }
}
