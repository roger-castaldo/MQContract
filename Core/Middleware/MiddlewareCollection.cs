using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Middleware;
using MQContract.Logging;
using System.Collections.Concurrent;

namespace MQContract.Middleware;

internal class MiddlewareCollection
{
    public enum InjectionPositions
    {
        Pre,
        Post
    };

    private readonly ConcurrentBag<IMiddleware> collection = [];
    private readonly ConcurrentDictionary<(Type middlewareType, InjectionPositions position), IEnumerable<IMiddleware>> injectableItems = [];
    private readonly ConcurrentDictionary<Type, IEnumerable<IMiddleware>> cache = [];
    private readonly ILogger logger;

    public MiddlewareCollection(ILogger logger, ChannelMapper? channelMapper, MessageContext messageContext, IMessageEncryptor? defaultMessageEncryptor, IServiceProvider? serviceProvider)
    {
        this.logger=logger;
        collection.Add(new ChannelMappingMiddleware(channelMapper));
        var compressionMiddleware = new CompressionMiddleware();
        RegisterInjectionMiddleware<IAfterEncodeMiddleware>(compressionMiddleware, InjectionPositions.Post);
        RegisterInjectionMiddleware<IBeforeDecodeMiddleware>(compressionMiddleware, InjectionPositions.Pre);
        var encryptionMiddleware = new EncryptionMiddleware(messageContext, defaultMessageEncryptor, serviceProvider);
        RegisterInjectionMiddleware<IAfterEncodeMiddleware>(encryptionMiddleware, InjectionPositions.Post);
        RegisterInjectionMiddleware<IBeforeDecodeMiddleware>(encryptionMiddleware, InjectionPositions.Pre);
    }

    public async Task RegisterMiddlewareInstanceAsync(object element,IEnumerable<MQContractMessageContext> contexts)
    {
        if (element is not IMiddleware middleware)
            throw new InvalidMiddlewareException(element.GetType());
        Logs.Lifetime.RegisteringMiddleware(logger, element.GetType());
        if (middleware is IMessageContextAwareMiddleware messageContextAwareMiddleware)
            await Task.WhenAll(contexts.Select(context => messageContextAwareMiddleware.ProcessMessagesFromMessageContextAsync(context.DefinedMessages).AsTask()));
        collection.Add(middleware);
        cache.Clear();
    }

    public void RegisterInjectionMiddleware<TMiddleware>(TMiddleware middleware, InjectionPositions position)
        where TMiddleware : IMiddleware
    {
        if (!injectableItems.TryGetValue((typeof(TMiddleware), position), out IEnumerable<IMiddleware>? list))
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
        cache.Clear();
    }

    public (IEnumerable<TGenericHandler> genericHandlers, IEnumerable<TSpecificHandler> specificHandlers) GetHandlers<TGenericHandler, TSpecificHandler>()
        where TGenericHandler : IMiddleware
        where TSpecificHandler : IMiddleware
        => (GetHandlers<TGenericHandler>(), GetHandlers<TSpecificHandler>());

    public IEnumerable<THandler> GetHandlers<THandler>()
        where THandler : IMiddleware
    {
        if (!cache.TryGetValue(typeof(THandler), out var handlers))
        {
            IEnumerable<THandler> enumHandlers;
            if (typeof(THandler).IsGenericType)
                enumHandlers = [.. collection.OfType<THandler>()];
            else
            {
                injectableItems.TryGetValue((typeof(THandler), InjectionPositions.Pre), out var preItems);
                injectableItems.TryGetValue((typeof(THandler), InjectionPositions.Post), out var postItems);
                enumHandlers = [
                    .. (preItems?? []).OfType<THandler>(),
                    .. collection.OfType<THandler>(),
                    .. (postItems ?? []).OfType<THandler>()
                ];
            }
            handlers = enumHandlers.OfType<IMiddleware>();
            cache.TryAdd(typeof(THandler), handlers);
        }
        return handlers.OfType<THandler>();
    }

    public async Task RegisterMessageContextAsync(MQContractMessageContext messageContext)
    {
        var tasks = collection
            .OfType<IMessageContextAwareMiddleware>()
            .Select(middleware => middleware.ProcessMessagesFromMessageContextAsync(messageContext.DefinedMessages).AsTask());
        await Task.WhenAll(tasks);
    }
}
