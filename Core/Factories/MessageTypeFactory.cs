using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Defaults;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Messages;
using MQContract.Messages;
using System.Reflection;
using System.Runtime.Loader;

namespace MQContract.Factories
{
    internal class MessageTypeFactory<T>
        : AConverter<T,T>, IMessageFactory<T>
    {

        private readonly IMessageEncoder? globalMessageEncoder;
        private readonly IMessageTypeEncoder<T>? messageEncoder;
        private readonly IEnumerable<IConversionPath<T>> converters;
        public bool IgnoreMessageHeader { get; private init; }

        private readonly string messageName = Utility.MessageTypeName<T>();
        private readonly string messageVersion = Utility.MessageVersionString<T>();
        public string? MessageChannel => typeof(T).GetCustomAttributes<MessageChannelAttribute>().Select(mc => mc.Name).FirstOrDefault();

        public MessageTypeFactory(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider, bool ignoreMessageHeader)
        {
            this.globalMessageEncoder = globalMessageEncoder;
            IgnoreMessageHeader = ignoreMessageHeader;
            var types = AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract
                            && Array.Exists(t.GetInterfaces(), iface => iface == typeof(IMessageTypeEncoder<T>)
                                || iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IMessageConverter<,>)));
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                });
            var encoderType = types
                .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IMessageTypeEncoder<T>)));
            messageEncoder = (IMessageTypeEncoder<T>?)((serviceProvider, encoderType, globalMessageEncoder) switch
            {
                (not null, not null, _) => ActivatorUtilities.CreateInstance(serviceProvider!, encoderType!),
                (null, not null, _) => Activator.CreateInstance(encoderType)!,
                (_, null, null) => new JsonEncoder<T>(),
                _ => null
            });
            converters = IgnoreMessageHeader
                ? []
                : ProduceConverters<T>(types, globalMessageEncoder, serviceProvider);
        }

        private static IEnumerable<IConversionPath<M>> ProduceConverters<M>(IEnumerable<Type> types, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            var paths = types
                .Where(t => Array.Exists(t.GetInterfaces(), iface => iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(IMessageConverter<,>)
                    && iface.GetGenericArguments()[1] == typeof(M)
                ))
                .Select(t => (IEnumerable<object>)[
                    (serviceProvider == null ?
                        Activator.CreateInstance(t)! :
                        ActivatorUtilities.CreateInstance(serviceProvider, t)
                    )
                ])
                .ToList();

            for (var x = 0; x<paths.Count; x++)
            {
                var conv = paths[x];
                var destType = ExtractGenericArguements(conv.First().GetType())[0];
                paths.AddRange(
                    [.. types
                    .Where(t => Array.Exists(t.GetInterfaces(), iface => iface.IsGenericType &&
                        iface.GetGenericTypeDefinition() == typeof(IMessageConverter<,>)
                        && iface.GetGenericArguments()[1] == destType
                        && !paths.Exists(path => Equals(ExtractGenericArguements(path.First().GetType())[0], iface.GetGenericArguments()[0]))
                    ))
                    .Select(t => conv.Prepend((serviceProvider == null ?
                        Activator.CreateInstance(t)! :
                        ActivatorUtilities.CreateInstance(serviceProvider, t)
                    )))]
                );
            }

            return paths
                .Select(path =>
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    var args = new object[] { path, types, globalMessageEncoder, serviceProvider };
#pragma warning restore CS8601 // Possible null reference assignment.
                    var type = typeof(ConversionPath<,>).MakeGenericType(
                        ExtractGenericArguements(path.First().GetType())[0],
                        typeof(M)
                    );
                    return (IConversionPath<M>)(serviceProvider==null ? Activator.CreateInstance(type, args)! : ActivatorUtilities.CreateInstance(serviceProvider, type, args)!);
                });
        }

        private static Type[] ExtractGenericArguements(Type t) => t.GetInterfaces().First(iface => iface.IsGenericType && iface.GetGenericTypeDefinition()==typeof(IMessageConverter<,>)).GetGenericArguments();

        public async ValueTask<ServiceMessage> ConvertMessageAsync(T message, bool ignoreChannel, string? channel, MessageHeader messageHeader)
        {
            if (string.IsNullOrWhiteSpace(channel)&&!ignoreChannel)
                throw new MessageChannelNullException();

            return new ServiceMessage(
                Guid.NewGuid().ToString(),
                $"{messageName}-{messageVersion}", 
                channel??string.Empty, 
                messageHeader,
                await (messageEncoder?.EncodeAsync(message)??globalMessageEncoder!.EncodeAsync<T>(message))
            );
        }

        protected override async ValueTask<T?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream)
        {
            if (!IgnoreMessageHeader)
#pragma warning disable S3236 // Caller information arguments should not be provided explicitly
                ArgumentNullException.ThrowIfNullOrWhiteSpace(message.MessageTypeID, nameof(message.MessageTypeID));
#pragma warning restore S3236 // Caller information arguments should not be provided explicitly
            if (Equals(ErrorServiceMessage.MessageTypeID, message.MessageTypeID))
                throw ErrorServiceMessage.DecodeError(message.Data);
            IConversionPath<T>? converter = null;
            T? result;
            if (IgnoreMessageHeader || ((IConversionPath<T>)this).IsMatch(message.MessageTypeID))
                result = await (messageEncoder?.DecodeAsync(new MemoryStream(message.Data.ToArray()))??globalMessageEncoder!.DecodeAsync<T>(new MemoryStream(message.Data.ToArray())));
            else
            {
                converter = converters.FirstOrDefault(conv => conv.IsMatch(message.MessageTypeID));
                if (converter==null)
                    throw new InvalidCastException();
                result = await converter.ConvertMessageAsync(logger, message, dataStream: new MemoryStream(message.Data.ToArray()));
            }
            if (Equals(result, default(T?)))
                throw new MessageConversionException(typeof(T), converter?.GetType()??GetType());
            return result;
        }
    }
}
