using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Messages;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;

namespace MQContract
{
    internal partial class MessageContext
    {
        [ExcludeFromCodeCoverage(Justification = "Simple record class used as a key in a dictionary, nothing to test")]
        private readonly record struct ConverterKey(string MessageID, Type MessageType);
        private readonly record struct ConverterInstance(object Instance, Type ConverterType, Type SourceType, Type DestinationType);

        private readonly ConcurrentDictionary<ConverterKey, Func<IEncodedMessage, ValueTask<object?>>?> cachedConverters = [];

        public Func<IEncodedMessage, ValueTask<object?>>? GetMessageConverter<TMessage>(string messageID, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            var key = new ConverterKey(messageID.ToUpperInvariant(), typeof(TMessage));
            if (!cachedConverters.TryGetValue(key, out var converter))
            {
                var messageDecode = GetDecodingCallback(messageID, globalMessageEncoder, serviceProvider);
                foreach (var context in contexts)
                {
                    converter = context.TryGetMessageConverter<TMessage>(messageID, messageDecode, serviceProvider);
                    if (converter!=null)
                        break;
                }
                cachedConverters.TryAdd(key, converter);
            }
            return converter;
        }

        internal void PrimeConverters<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            if (!contexts.Any(context => context.IsMessageCodeGenerated<TMessage>()) && DynamicCodeGate.IsSupported)
                ExtractConvertersThroughReflection<TMessage>(globalMessageEncoder, serviceProvider);
        }

        [RequiresDynamicCode("Uses unbounded reflection to discover converters, if AOT and no usage of UseMqContractAttribute to autogenerate code the converter will return null and fail to convert")]
        private void ExtractConvertersThroughReflection<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            var types = AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract
                            && Array.Exists(t.GetInterfaces(), iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IMessageConverter<,>)));
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                });
            foreach (var primaryConverter in GetConvertersForMessageType(typeof(TMessage), types, serviceProvider))
            {
                var pkey = ExtractConverterKey<TMessage>(primaryConverter.SourceType);
                cachedConverters.TryRemove(pkey, out _);
                CacheConverterChains<TMessage>(primaryConverter.SourceType, [primaryConverter], types, globalMessageEncoder, serviceProvider);
            }
        }

        private void CacheConverterChains<TMessage>(Type sourceType, IEnumerable<ConverterInstance> converters, IEnumerable<Type> types, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            var pkey = ExtractConverterKey<TMessage>(sourceType);
            if (!cachedConverters.ContainsKey(pkey))
            {
                cachedConverters.TryAdd(pkey, ProduceConverterCall(sourceType, converters, globalMessageEncoder, serviceProvider));
                foreach (var converter in GetConvertersForMessageType(sourceType, types, serviceProvider))
                    CacheConverterChains<TMessage>(converter.SourceType, new ConverterInstance[] { converter }.Concat(converters), types, globalMessageEncoder, serviceProvider);
            }
        }

        private Func<IEncodedMessage, ValueTask<object?>> ProduceConverterCall(Type sourceType, IEnumerable<ConverterInstance> converters, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            var messageDecode = GetDecodingCallback(MessageID(sourceType), globalMessageEncoder, serviceProvider);
            return async message =>
            {
                var result = await messageDecode(message);
                if (result == null) return null;
                foreach (var converter in converters)
                {
                    result = await Utility.InvokeMethodAsync(
                        converter.ConverterType.GetMethod("ConvertAsync")!,
                        converter.Instance,
                        [result]
                    );
                    if (result==null) break;
                }
                return result;
            };
        }

        private ConverterKey ExtractConverterKey<TMessage>(Type sourceType)
            => new(MessageID(sourceType).ToUpperInvariant(), typeof(TMessage));

        private static IEnumerable<ConverterInstance> GetConvertersForMessageType(Type messageType, IEnumerable<Type> types, IServiceProvider? serviceProvider)
            => types.SelectMany(t =>
                    t.GetInterfaces()
                    .Where(iface =>
                        iface.IsGenericType &&
                        iface.GetGenericTypeDefinition() == typeof(IMessageConverter<,>) &&
                        iface.GetGenericArguments()[1] == messageType)
                    .Select(iface => new ConverterInstance(
                        (serviceProvider==null ? Activator.CreateInstance(t) : ActivatorUtilities.CreateInstance(serviceProvider, t))!,
                        iface,
                        iface.GetGenericArguments()[0],
                        iface.GetGenericArguments()[1]
                    ))
                );
    }
}
