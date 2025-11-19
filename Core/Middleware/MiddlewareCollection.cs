using Microsoft.Extensions.Logging;
using MQContract.Extensions;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Middleware;
using System.Collections.Concurrent;
using System.Reflection;

namespace MQContract.Middleware
{
    internal class MiddlewareCollection
    {
        private static readonly Type[] validMiddlewareTypes = [
            typeof(IAfterDecodeMiddleware),
            typeof(IAfterDecodeSpecificTypeMiddleware<>),
            typeof(IAfterEncodeMiddleware),
            typeof(IBeforeDecodeMiddleware),
            typeof(IBeforeEncodeMiddleware),
            typeof(IBeforeEncodeSpecificTypeMiddleware<>)
        ];
        public enum InjectionPositions
        {
            Pre,
            Post
        };

        private readonly ConcurrentBag<object> collection = [];
        private readonly ConcurrentDictionary<(Type middlewareType,InjectionPositions position), IEnumerable<object>> injectableItems = [];
        private readonly ILogger? logger;

        public MiddlewareCollection(ILogger? logger,ChannelMapper? channelMapper,IMessageEncryptor? defaultMessageEncryptor, IServiceProvider? serviceProvider)
        {
            this.logger=logger;
            collection.Add(new ChannelMappingMiddleware(channelMapper));
            var compressionMiddleware = new CompressionMiddleware();
            RegisterInjectionMiddleware<IAfterEncodeMiddleware>(compressionMiddleware, InjectionPositions.Post);
            RegisterInjectionMiddleware<IBeforeDecodeMiddleware>(compressionMiddleware, InjectionPositions.Pre);
            var encryptionMiddleware = new EncryptionMiddleware(defaultMessageEncryptor, serviceProvider);
            RegisterInjectionMiddleware<IAfterEncodeMiddleware>(encryptionMiddleware, InjectionPositions.Post);
            RegisterInjectionMiddleware<IBeforeDecodeMiddleware>(encryptionMiddleware, InjectionPositions.Pre);
        }

        public void RegisterMiddlewareInstance(object element)
        {
            if (!Array.Exists(element.GetType().GetInterfaces(), (i) => validMiddlewareTypes.Contains((i.IsGenericType ? i.GetGenericTypeDefinition() : i))))
                throw new InvalidMiddlewareException(element.GetType());
            logger?.LogDebugChecked("Registering middleware of type {Type}", element.GetType());
            collection.Add(element);
        }

        public void RegisterInjectionMiddleware<TMiddleware>(TMiddleware middleware,InjectionPositions position)
            where TMiddleware : IMiddleware
        {
            if (!injectableItems.TryGetValue((typeof(TMiddleware), position), out IEnumerable<object>? list))
            {
                list = [];
                injectableItems.TryAdd((typeof(TMiddleware), position), list);
            }
            injectableItems.TryUpdate((typeof(TMiddleware), position),
                [.. list.Append(middleware)
                    .Select((item, index) => new {Item=item,OriginalIndex=index})
                    .OrderBy(x=>Utility.GetCustomAttribute<MiddlewareInjectionOrderAttribute<TMiddleware>>(x.Item.GetType())?.GetIndex(position)??0)
                    .ThenBy(x=>x.OriginalIndex)
                    .Select(x=>x.Item)
                ]
                , list);
        }

        public (IEnumerable<TGenericHandler> genericHandlers, IEnumerable<TSpecificHandler> specificHandlers) GetHandlers<TGenericHandler, TSpecificHandler>()
            => (GetHandlers<TGenericHandler>(), collection.OfType<TSpecificHandler>().ToArray());

        public IEnumerable<THandler> GetHandlers<THandler>()
        {
            injectableItems.TryGetValue((typeof(THandler), InjectionPositions.Pre), out var preItems);
            injectableItems.TryGetValue((typeof(THandler), InjectionPositions.Post), out var postItems);
            return [
                .. (preItems??[]).OfType<THandler>().ToArray(),
                .. collection.OfType<THandler>().ToArray(),
                .. (postItems??[]).OfType<THandler>().ToArray()
            ];
        }
    }
}
