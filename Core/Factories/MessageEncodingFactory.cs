using Microsoft.Extensions.DependencyInjection;
using MQContract.Defaults;
using MQContract.Interfaces.Encoding;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace MQContract.Factories
{
    internal static partial class MessageEncodingFactory
    {
        private static (Func<TMessage, ValueTask<byte[]>> encodeMessage, Func<Stream, ValueTask<TMessage?>> decodeMessage) ProduceCallbacks<TMessage>(object encoder)
        {
            if (encoder is IMessageTypeEncoder<TMessage> typeEncoder)
                return ((message) => typeEncoder.EncodeAsync(message), (stream) => typeEncoder.DecodeAsync(stream));
            var messageEncoder = (IMessageEncoder)encoder;
            return ((message) => messageEncoder.EncodeAsync<TMessage>(message), (stream) => messageEncoder.DecodeAsync<TMessage>(stream));
        }

        public static (Func<TMessage, ValueTask<byte[]>> encodeMessage, Func<Stream, ValueTask<TMessage?>> decodeMessage) GetCallbacks<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            object? internalEncoder = (typeof(TMessage), globalMessageEncoder, serviceProvider) switch
            {
                (Type t, _, _) when t == typeof(bool) => new BooleanEncoder(),
                (Type t, _, _) when t == typeof(byte[]) => new ByteArrayEncoder(),
                (Type t, _, _) when t == typeof(byte) => new ByteEncoder(),
                (Type t, _, _) when t == typeof(char) => new CharEncoder(),
                (Type t, _, _) when t == typeof(decimal) => new DecimalEncoder(),
                (Type t, _, _) when t == typeof(double) => new DoubleEncoder(),
                (Type t, _, _) when t == typeof(float) => new FloatEncoder(),
                (Type t, _, _) when t == typeof(Half) => new HalfEncoder(),
                (Type t, _, _) when t == typeof(int) => new IntEncoder(),
                (Type t, _, _) when t == typeof(long) => new LongEncoder(),
                (Type t, _, _) when t == typeof(short) => new ShortEncoder(),
                (Type t, _, _) when t == typeof(string) => new StringEncoder(),
                (Type t, _, _) when t == typeof(uint) => new UIntEncoder(),
                (Type t, _, _) when t == typeof(ulong) => new ULongEncoder(),
                (Type t, _, _) when t == typeof(ushort) => new UShortEncoder(),
                _ => null
            };
            if (internalEncoder!=null)
                return ProduceCallbacks<TMessage>(internalEncoder);
            var specificEncoder = TryGetMessageEncoder<TMessage>(globalMessageEncoder, serviceProvider);
            if (specificEncoder!=null)
                return ProduceCallbacks<TMessage>(specificEncoder);
            if (RuntimeFeature.IsDynamicCodeSupported)
                return ProduceCallbacks<TMessage>(ExtractEncoderThroughReflection<TMessage>(globalMessageEncoder, serviceProvider));
            return ProduceCallbacks<TMessage>((globalMessageEncoder == null ? new JsonEncoder<TMessage>() : globalMessageEncoder));
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
                                    .Where(t => !t.IsInterface && !t.IsInterface && Array.Exists(t.GetInterfaces(), iface => iface==typeof(IMessageTypeEncoder<TMessage>)));
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
    }
}
