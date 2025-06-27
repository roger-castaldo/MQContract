using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Middleware;
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

        private readonly ReaderWriterLockSlim dataLock = new();
        private readonly List<object> collection = [];
        private readonly Dictionary<Type, Dictionary<InjectionPositions, List<object>>> injectableItems = [];
        private readonly ILogger? logger;

        public MiddlewareCollection(ILogger? logger,ChannelMapper? channelMapper,IMessageEncryptor? defaultMessageEncryptor, IServiceProvider? serviceProvider)
        {
            this.logger=logger;
            foreach (var type in validMiddlewareTypes.Where(t=>!t.IsGenericTypeDefinition))
            {
                var directions = new Dictionary<InjectionPositions, List<object>>();
                foreach (var position in Enum.GetValues<InjectionPositions>())
                    directions.Add(position, []);
                injectableItems.Add(type, directions);  
            }
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
            logger?.LogDebug("Registering middleware of type {Type}", element.GetType());
            dataLock.EnterWriteLock();
            collection.Add(element);
            dataLock.ExitWriteLock();
        }

        public void RegisterInjectionMiddleware<T>(T middleware,InjectionPositions position)
            where T : IMiddleware
        {
            dataLock.EnterWriteLock();
            var list = injectableItems[typeof(T)][position];
            list.Add(middleware);
            injectableItems[typeof(T)].Remove(position);
            injectableItems[typeof(T)].Add(position,
                [.. list
                .Select((item, index) => new {Item=item,OriginalIndex=index})
                .OrderBy(x=>x.Item.GetType().GetCustomAttribute<MiddlewareInjectionOrderAttribute<T>>()?.GetIndex(position)??0)
                .ThenBy(x=>x.OriginalIndex)
                .Select(x=>x.Item)]
            );
            dataLock.ExitWriteLock();
        }

        public (IEnumerable<G> genericHandlers, IEnumerable<S> specificHandlers) GetHandlers<G, S>()
        {
            dataLock.EnterReadLock();
            var genericHandlers = GetHandlers<G>(false);
            var specificHandlers = collection.OfType<S>().ToArray();
            dataLock.ExitReadLock();
            return (genericHandlers, specificHandlers);
        }

        private IEnumerable<G> GetHandlers<G>(bool withLock)
        {
            if (withLock)
                dataLock.EnterReadLock();
            IEnumerable<G> genericHandlers = [
                .. injectableItems[typeof(G)][InjectionPositions.Pre].OfType<G>().ToArray(),
                .. collection.OfType<G>().ToArray(),
                .. injectableItems[typeof(G)][InjectionPositions.Post].OfType<G>().ToArray()
            ];
            if (withLock)
                dataLock.ExitReadLock();
            return genericHandlers;
        }

        public IEnumerable<G> GetHandlers<G>()
            => GetHandlers<G>(true);
    }
}
