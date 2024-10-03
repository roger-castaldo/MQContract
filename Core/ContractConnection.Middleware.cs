using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using MQContract.Middleware;

namespace MQContract
{
    public sealed partial class ContractConnection
    {
        private ContractConnection RegisterMiddleware(object element)
        {
            lock (middleware)
            {
                middleware.Add(element);
            }
            return this;
        }

        private ContractConnection RegisterMiddleware(Type type)
            => RegisterMiddleware((serviceProvider == null ? Activator.CreateInstance(type) : ActivatorUtilities.CreateInstance(serviceProvider, type))!);

        IContractConnection IContractConnection.RegisterMiddleware<T>()
            => RegisterMiddleware(typeof(T));
        IContractConnection IContractConnection.RegisterMiddleware<T>(Func<T> constructInstance)
            => RegisterMiddleware(constructInstance());
        IContractConnection IContractConnection.RegisterMiddleware<T, M>()
            => RegisterMiddleware(typeof(T));
        IContractConnection IContractConnection.RegisterMiddleware<T, M>(Func<T> constructInstance)
            => RegisterMiddleware(constructInstance());

        private async ValueTask<(T message,string? channel,MessageHeader messageHeader)> BeforeMessageEncodeAsync<T>(IContext context, T message, string? channel, MessageHeader messageHeader)
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
                (message,channel,messageHeader) = await handler.BeforeMessageEncodeAsync<T>(context,message, channel, messageHeader);
            foreach(var handler in specificHandlers)
                (message,channel,messageHeader) = await handler.BeforeMessageEncodeAsync(context,message, channel, messageHeader);  
            return (message, channel, messageHeader);
        }

        private async ValueTask<ServiceMessage> AfterMessageEncodeAsync<T>(IContext context,ServiceMessage message)
        {
            IAfterEncodeMiddleware[] genericHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IAfterEncodeMiddleware>().ToArray();
            }
            foreach(var handler in genericHandlers)
                message = await handler.AfterMessageEncodeAsync(typeof(T),context,message);
            return message;
        }

        private async ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID,string messageChannel, ReadOnlyMemory<byte> data)
        {
            IBeforeDecodeMiddleware[] genericHandlers;
            lock (middleware)
            {
                genericHandlers = middleware.OfType<IBeforeDecodeMiddleware>().ToArray();
            }
            foreach (var handler in genericHandlers)
                (messageHeader,data) = await handler.BeforeMessageDecodeAsync(context,id,messageHeader,messageTypeID,messageChannel,data);
            return (messageHeader,data);
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

        private async ValueTask<ServiceMessage> ProduceServiceMessageAsync<T>(ChannelMapper.MapTypes mapType,IMessageFactory<T> messageFactory, T message, bool ignoreChannel, string? channel = null, MessageHeader? messageHeader = null) 
            where T : class
        {
            var context = new Context(mapType);
            (message, channel, messageHeader) = await BeforeMessageEncodeAsync<T>(context, message, channel??messageFactory.MessageChannel, messageHeader??new([]));
            return await AfterMessageEncodeAsync<T>(context,
                await messageFactory.ConvertMessageAsync(message, ignoreChannel, channel, messageHeader)
            );
        }
        private async ValueTask<(T message,MessageHeader header)> DecodeServiceMessageAsync<T>(ChannelMapper.MapTypes mapType, IMessageFactory<T> messageFactory, ReceivedServiceMessage message)
            where T : class
        {
            var context = new Context(mapType);
            (var messageHeader, var data) = await BeforeMessageDecodeAsync(context, message.ID, message.Header, message.MessageTypeID, message.Channel, message.Data);
            var taskMessage = await messageFactory.ConvertMessageAsync(logger, new ReceivedServiceMessage(message.ID, message.MessageTypeID, message.Channel, messageHeader, data, message.Acknowledge))
                                ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(T).FullName}");
            return await AfterMessageDecodeAsync<T>(context, taskMessage!, message.ID, messageHeader, message.ReceivedTimestamp, DateTime.Now);
        }
    }
}
