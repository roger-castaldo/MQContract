using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Factories;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using MQContract.Middleware;
using System.Diagnostics.Metrics;

namespace MQContract.Connections
{
    internal abstract class AConnection(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) : 
        IDisposable,IAsyncDisposable
    {
        private bool disposedValue;
        protected readonly Guid indentifier = Guid.NewGuid();
        protected readonly SemaphoreSlim dataLock = new(1, 1);
        private readonly List<object> middleware = [new ChannelMappingMiddleware(channelMapper)];
        private IEnumerable<IMessageTypeFactory> typeFactories = [];
        protected ILogger? Logger => logger;

        protected IMessageFactory<T> GetMessageFactory<T>(uint? maxMessageBodySize,bool ignoreMessageHeader = false) where T : class
        {
            dataLock.Wait();
            var result = (IMessageFactory<T>?)typeFactories.FirstOrDefault(fact => fact.GetType().GetGenericArguments()[0] == typeof(T));
            dataLock.Release();
            if (result == null)
            {
                result = new MessageTypeFactory<T>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, ignoreMessageHeader, maxMessageBodySize);
                dataLock.Wait();
                if (!typeFactories.Any(fact => fact.GetType().GetGenericArguments()[0] == typeof(T) && fact.IgnoreMessageHeader == ignoreMessageHeader))
                    typeFactories = typeFactories.Concat([result]);
                dataLock.Release();
            }
            return result;
        }

        protected ValueTask<string> MapChannel(ChannelMapper.MapTypes mapType, string originalChannel)
            => channelMapper?.MapChannel(mapType, originalChannel) ?? ValueTask.FromResult(originalChannel);

        #region Middleware

        private AConnection RegisterMiddleware(object element)
        {
            dataLock.Wait();
            middleware.Add(element);
            dataLock.Release();
            return this;
        }

        protected AConnection RegisterMiddleware(Type type)
            => RegisterMiddleware((serviceProvider == null ? Activator.CreateInstance(type) : ActivatorUtilities.CreateInstance(serviceProvider, type))!);
        protected AConnection RegisterMiddleware<T>()
            => RegisterMiddleware(typeof(T));
        protected AConnection RegisterMiddleware<T>(Func<T> constructInstance)
            => RegisterMiddleware(constructInstance());
        protected AConnection RegisterMiddleware<T, M>()
            => RegisterMiddleware(typeof(T));
        protected AConnection RegisterMiddleware<T, M>(Func<T> constructInstance)
            => RegisterMiddleware(constructInstance());

        private async ValueTask<(T message, string? channel, MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
            where T : class
        {
            IBeforeEncodeMiddleware[] genericHandlers;
            IBeforeEncodeSpecificTypeMiddleware<T>[] specificHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IBeforeEncodeMiddleware>().ToArray();
                specificHandlers = middleware.OfType<IBeforeEncodeSpecificTypeMiddleware<T>>().ToArray();
            }
            foreach (var handler in genericHandlers)
                (message, channel, messageHeader) = await handler.BeforeMessageEncodeAsync<T>(context, message, channel, messageHeader);
            foreach (var handler in specificHandlers)
                (message, channel, messageHeader) = await handler.BeforeMessageEncodeAsync(context, message, channel, messageHeader);
            return (message, channel, messageHeader);
        }

        private async ValueTask<ServiceMessage> AfterMessageEncodeAsync<T>(IContext context, ServiceMessage message)
        {
            IAfterEncodeMiddleware[] genericHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IAfterEncodeMiddleware>().ToArray();
            }
            foreach (var handler in genericHandlers)
                message = await handler.AfterMessageEncodeAsync(typeof(T), context, message);
            return message;
        }

        private async ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            IBeforeDecodeMiddleware[] genericHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IBeforeDecodeMiddleware>().ToArray();
            }
            foreach (var handler in genericHandlers)
                (messageHeader, data) = await handler.BeforeMessageDecodeAsync(context, id, messageHeader, messageTypeID, messageChannel, data);
            return (messageHeader, data);
        }

        private async ValueTask<(T message, MessageHeader messageHeader)> AfterMessageDecodeAsync<T>(IContext context, T message, string ID, MessageHeader messageHeader, DateTime receivedTimestamp, DateTime processedTimeStamp)
            where T : class
        {
            IAfterDecodeMiddleware[] genericHandlers;
            IAfterDecodeSpecificTypeMiddleware<T>[] specificHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IAfterDecodeMiddleware>().ToArray();
                specificHandlers = middleware.OfType<IAfterDecodeSpecificTypeMiddleware<T>>().ToArray();
            }
            foreach (var handler in genericHandlers)
                (message, messageHeader) = await handler.AfterMessageDecodeAsync<T>(context, message, ID, messageHeader, receivedTimestamp, processedTimeStamp);
            foreach (var handler in specificHandlers)
                (message, messageHeader) = await handler.AfterMessageDecodeAsync(context, message, ID, messageHeader, receivedTimestamp, processedTimeStamp);
            return (message, messageHeader);
        }

        protected async ValueTask<ServiceMessage> ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes mapType, IMessageFactory<T> messageFactory, T message, bool ignoreChannel, string? channel = null, MessageHeader? messageHeader = null)
            where T : class
        {
            var context = new Context(mapType);
            (message, channel, messageHeader) = await BeforeMessageEncodeAsync<T>(context, message, channel??messageFactory.MessageChannel, messageHeader??new([]));
            return await AfterMessageEncodeAsync<T>(context,
                await messageFactory.ConvertMessageAsync(message, ignoreChannel, channel, messageHeader)
            );
        }
        protected async ValueTask<(T message, MessageHeader header)> DecodeServiceMessageAsync<T>(ChannelMapper.MapTypes mapType, IMessageFactory<T> messageFactory, ReceivedServiceMessage message)
            where T : class
        {
            var context = new Context(mapType);
            (var messageHeader, var data) = await BeforeMessageDecodeAsync(context, message.ID, message.Header, message.MessageTypeID, message.Channel, message.Data);
            var taskMessage = await messageFactory.ConvertMessageAsync(logger, new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, messageHeader, data, message.Acknowledge))
                                ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(T).FullName}");
            return await AfterMessageDecodeAsync<T>(context, taskMessage!, message.ID, messageHeader, message.ReceivedTimestamp, DateTime.Now);
        }
        #endregion

        #region Metrics
        protected AConnection AddMetrics(Meter? meter, bool useInternal)
        {
            dataLock.Wait();
            middleware.Insert(0, new MetricsMiddleware(meter, useInternal));
            dataLock.Release();
            return this;
        }

        private MetricsMiddleware? MetricsMiddleware
        {
            get
            {
                MetricsMiddleware? metricsMiddleware;
                lock (middleware)
                {
                    metricsMiddleware = middleware.OfType<MetricsMiddleware>().FirstOrDefault();
                }
                return metricsMiddleware;
            }
        }

        protected IContractMetric? GetSnapshot(bool sent)
            => MetricsMiddleware?.GetSnapshot(sent);
        protected IContractMetric? GetSnapshot(Type messageType, bool sent)
            => MetricsMiddleware?.GetSnapshot(messageType, sent);
        protected IContractMetric? GetSnapshot<T>(bool sent)
            => MetricsMiddleware?.GetSnapshot(typeof(T), sent);
        protected IContractMetric? GetSnapshot(string channel, bool sent)
            => MetricsMiddleware?.GetSnapshot(channel, sent);
        #endregion

        protected abstract void InternalDispose();
        protected abstract ValueTask InternalDisposeAsync();

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    InternalDispose();
                dataLock.Dispose();
                disposedValue =true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await InternalDisposeAsync();
            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}
