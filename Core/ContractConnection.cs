using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Defaults;
using MQContract.Factories;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Factories;
using MQContract.Messages;
using MQContract.ServiceAbstractions;
using MQContract.ServiceAbstractions.Messages;
using System.Reflection;

namespace MQContract
{
    public class ContractConnection(IMessageServiceConnection serviceConnection, 
        IMessageEncoder? defaultMessageEncoder = null, 
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider=null,
        ILogger? logger=null)
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
                result = new MessageTypeFactory<T>(defaultMessageEncoder, defaultMessageEncryptor,serviceProvider,ignoreMessageHeader,serviceConnection.MaxMessageBodySize);
                dataLock.Wait();
                if (!typeFactories.Any(fact => fact.GetType().GetGenericArguments()[0]==typeof(T) && fact.IgnoreMessageHeader==ignoreMessageHeader))
                    typeFactories = typeFactories.Concat([(IMessageTypeFactory)result]);
                dataLock.Release();
            }
            return result;
        }

        public Task<IPingResult> PingAsync()
            => serviceConnection.PingAsync();

        public Task<ITransmissionResult> PublishAsync<T>(T message,TimeSpan? timeout=null, string? channel = null,IMessageHeader? messageHeader=null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken()) 
            where T : class
            => serviceConnection.PublishAsync(
                ProduceServiceMessage<T>(message,channel:channel,messageHeader:messageHeader),
                timeout??TimeSpan.FromMilliseconds(typeof(T).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.Value??serviceConnection.DefaultTimout.TotalMilliseconds),
                options,
                cancellationToken
            );

        private IServiceMessage ProduceServiceMessage<T>(T message, string? channel = null, IMessageHeader? messageHeader = null) where T : class
            => GetMessageFactory<T>().ConvertMessage(message, channel, messageHeader);

        public async Task<IQueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout=null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken()) 
            where Q : class 
            where R : class
            => ProduceResultAsync<R>(await serviceConnection.QueryAsync(
                ProduceServiceMessage<Q>(message),
                timeout??TimeSpan.FromMilliseconds(typeof(Q).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.Value??serviceConnection.DefaultTimout.TotalMilliseconds),
                options,
                cancellationToken
            ));

        private QueryResult<R> ProduceResultAsync<R>(IServiceQueryResult queryResult) where R : class
            => new QueryResult<R>(
                    queryResult.IsError ? default : GetMessageFactory<R>().ConvertMessage(logger, queryResult),
                    queryResult.Header,
                    queryResult.MessageID,
                    queryResult.IsError,
                    queryResult.Error
                );
    }
}
