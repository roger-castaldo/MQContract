using Microsoft.Extensions.DependencyInjection;
using MQContract.Defaults;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Messages;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;

namespace MQContract
{
    internal partial class MessageContext
    {
        private static (Func<TMessage, ValueTask<byte[]>> encodeMessage, Func<Stream, ValueTask<TMessage?>> decodeMessage) ProduceCallbacks<TMessage>(object encoder)
        {
            if (encoder is IMessageTypeEncoder<TMessage> typeEncoder)
                return ((message) => typeEncoder.EncodeAsync(message), (stream) => typeEncoder.DecodeAsync(stream));
            var messageEncoder = (IMessageEncoder)encoder;
            return ((message) => messageEncoder.EncodeAsync<TMessage>(message), (stream) => messageEncoder.DecodeAsync<TMessage>(stream));
        }

        [RequiresDynamicCode("Uses unbounded reflection to discover encoders, if AOT and no usage of UseMqContractAttribute to autogenerate code, will result in falling back to Json Encoding as default")]
        private static object ExtractEncoderThroughReflection<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            var specificEncoderType = AssemblyLoadContext.All
                            .SelectMany(context => context.Assemblies)
                            .SelectMany(assembly =>
                            {
                                try
                                {
                                    return assembly.GetTypes()
                                    .Where(t => !t.IsInterface && !t.IsAbstract && Array.Exists(t.GetInterfaces(), iface => iface==typeof(IMessageTypeEncoder<TMessage>)));
                                }
                                catch (Exception)
                                {
                                    return [];
                                }
                            })
                            .FirstOrDefault();
            return (serviceProvider, specificEncoderType, globalMessageEncoder) switch
            {
                (not null, not null, _) => ActivatorUtilities.CreateInstance(serviceProvider!, specificEncoderType!)!,
                (null, not null, _) => Activator.CreateInstance(specificEncoderType!)!,
                (_, null, null) => new JsonEncoder<TMessage>(),
                _ => globalMessageEncoder!
            };
        }

        public (Func<TMessage, ValueTask<byte[]>> encodeMessage, Func<Stream, ValueTask<TMessage?>> decodeMessage) GetEncodingCallbacks<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            foreach(var context in contexts)
            {
                var specificEncoder = context.TryGetMessageEncoder<TMessage>(globalMessageEncoder, serviceProvider);
                if (specificEncoder!=null)
                    return ProduceCallbacks<TMessage>(specificEncoder);
            }
            if (DynamicCodeGate.IsSupported)
                return ProduceCallbacks<TMessage>(ExtractEncoderThroughReflection<TMessage>(globalMessageEncoder, serviceProvider));
            return ProduceCallbacks<TMessage>((globalMessageEncoder == null ? new JsonEncoder<TMessage>() : globalMessageEncoder));
        }

        public Func<IEncodedMessage, ValueTask<object?>> GetDecodingCallback(string messageID, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            foreach(var context in contexts)
            {
                var specificEncoder = context.TryGetDecodingCallback(messageID, globalMessageEncoder, serviceProvider);
                if (specificEncoder!=null)
                    return specificEncoder;
            }
            if (DynamicCodeGate.IsSupported)
                return ExtractDecodeThroughReflection(messageID, globalMessageEncoder, serviceProvider);
            return ProduceDecodingCallback(null, globalMessageEncoder);
        }

        private static Func<IEncodedMessage, ValueTask<object?>> ProduceDecodingCallback(Type? messageType, IMessageEncoder? globalMessageEncoder)
        {
            Func<Func<IEncodedMessage, ValueTask<object?>>> produce = (messageType, globalMessageEncoder, DynamicCodeGate.IsSupported) switch
            {
                (not null, not null, true) => () =>
                {
                    var method = typeof(IMessageEncoder).GetMethod(nameof(IMessageEncoder.DecodeAsync))!.MakeGenericMethod(messageType);
                    return async (IEncodedMessage message) =>
                    {
                        using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                        return await Utility.InvokeMethodAsync(
                            method,
                            globalMessageEncoder,
                            [ms]
                        );
                    };
                },
                (not null, null, true) => () =>
                {
                    var jEncoder = Activator.CreateInstance(typeof(JsonEncoder<>).MakeGenericType(messageType))!;
                    var method = typeof(JsonEncoder<>).MakeGenericType(messageType).GetMethod("DecodeAsync")!;
                    return async (IEncodedMessage message) =>
                    {
                        using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                        return await Utility.InvokeMethodAsync(
                            method,
                            jEncoder,
                            [ms]
                        );
                    };
                },
                (_, not null, false) => () =>
                {
                    return async (IEncodedMessage message) =>
                    {
                        using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                        return await globalMessageEncoder.DecodeAsync<object>(ms);
                    };
                },
                _ => () =>
                {
                    var jEncoder = new JsonEncoder<object>();
                    return async (IEncodedMessage message) =>
                    {
                        using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                        return await jEncoder.DecodeAsync(ms);
                    };
                }
            };
            return produce();
        }
        

        [RequiresDynamicCode("Uses unbounded reflection to discover encoders, if AOT and no usage of UseMqContractAttribute to autogenerate code for the encoders used")]
        private Func<IEncodedMessage, ValueTask<object?>> ExtractDecodeThroughReflection(string messageID, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            var messageType = AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract
                            && string.Equals(MessageID(t), messageID, StringComparison.InvariantCultureIgnoreCase));
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                })
                .FirstOrDefault();
            var encoderType = AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract
                            && Array.Exists(t.GetInterfaces(), iface => {
                                    if (iface.IsGenericType
                                    && iface.GetGenericTypeDefinition() == typeof(IMessageTypeEncoder<>)
                                    && string.Equals(MessageID(iface.GetGenericArguments()[0]), messageID, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    messageType = iface.GetGenericArguments()[0];
                                    return true;
                                }
                                return false;
                                }));
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                })
                .FirstOrDefault();
            if (encoderType!=null)
            {
                var encoder = (serviceProvider==null ? Activator.CreateInstance(encoderType) : ActivatorUtilities.CreateInstance(serviceProvider, encoderType))!;
                return async message =>
                {
                    using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                    return await Utility.InvokeMethodAsync(
                        typeof(IMessageTypeEncoder<>).MakeGenericType(messageType!).GetMethod("DecodeAsync")!,
                        encoder,
                        [ms]
                    );
                };
            }
            return ProduceDecodingCallback(messageType, globalMessageEncoder);
        }
    }
}
