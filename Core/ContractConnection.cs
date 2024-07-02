﻿using Microsoft.Extensions.Logging;
using MQContract.Attributes;
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
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null)
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

        public Task<IPingResult> PingAsync()
            => serviceConnection.PingAsync();

        public Task<ITransmissionResult> PublishAsync<T>(T message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where T : class
            => serviceConnection.PublishAsync(
                ProduceServiceMessage<T>(message, channel: channel, messageHeader: messageHeader),
                timeout??TimeSpan.FromMilliseconds(typeof(T).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.Value??serviceConnection.DefaultTimout.TotalMilliseconds),
                options,
                cancellationToken
            );

        private IServiceMessage ProduceServiceMessage<T>(T message, string? channel = null, IMessageHeader? messageHeader = null) where T : class
            => GetMessageFactory<T>().ConvertMessage(message, channel, messageHeader);

        private async Task<IQueryResult<R>> ExecuteQueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class
            => ProduceResultAsync<R>(await serviceConnection.QueryAsync(
                ProduceServiceMessage<Q>(message, channel: channel, messageHeader: messageHeader),
                timeout??TimeSpan.FromMilliseconds(typeof(Q).GetCustomAttribute<MessageResponseTimeoutAttribute>()?.Value??serviceConnection.DefaultTimout.TotalMilliseconds),
                options,
                cancellationToken
            ));

        public async Task<IQueryResult<R>> QueryAsync<Q, R>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken())
            where Q : class
            where R : class
            => await ExecuteQueryAsync<Q, R>(message, timeout: timeout, channel: channel, messageHeader: messageHeader, options: options, cancellationToken: cancellationToken);

        public async Task<IQueryResult<object>> QueryAsync<Q>(Q message, TimeSpan? timeout = null, string? channel = null, IMessageHeader? messageHeader = null,
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
            return (IQueryResult<object>)((dynamic)task).Result;
        }

        private QueryResult<R> ProduceResultAsync<R>(IServiceQueryResult queryResult) where R : class
            => new(
                    queryResult.IsError ? default : GetMessageFactory<R>().ConvertMessage(logger, queryResult),
                    queryResult.Header,
                    queryResult.MessageID,
                    queryResult.IsError,
                    queryResult.Error
                );
    }
}
