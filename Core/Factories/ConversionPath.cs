using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Defaults;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Messages;
using MQContract.Messages;

namespace MQContract.Factories
{
    internal class ConversionPath<T, V> : IConversionPath<V>
        where T : class
        where V : class
    {
        private readonly IEnumerable<object> path;
        private readonly IMessageTypeEncoder<T> messageEncoder;
        private readonly IMessageTypeEncryptor<T> messageEncryptor;
        private readonly IMessageEncoder? globalMessageEncoder;
        private readonly IMessageEncryptor? globalMessageEncryptor;

        public ConversionPath(IEnumerable<object> path, IEnumerable<Type> types, IMessageEncoder? globalMessageEncoder, IMessageEncryptor? globalMessageEncryptor, IServiceProvider? serviceProvider)
        {
            this.path = path;
            this.globalMessageEncoder = globalMessageEncoder;
            this.globalMessageEncryptor = globalMessageEncryptor;
            var encoderType = types
                .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IMessageTypeEncoder<T>)), typeof(JsonEncoder<T>));
            var encryptorType = types
                .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IMessageTypeEncryptor<T>)), typeof(NonEncryptor<T>));
            messageEncoder = (IMessageTypeEncoder<T>)(serviceProvider!=null ? ActivatorUtilities.CreateInstance(serviceProvider, encoderType) : Activator.CreateInstance(encoderType)!);
            messageEncryptor = (IMessageTypeEncryptor<T>)(serviceProvider!=null ? ActivatorUtilities.CreateInstance(serviceProvider, encryptorType) : Activator.CreateInstance(encryptorType)!);
        }

        public V? ConvertMessage(ILogger? logger, IEncodedMessage message, Stream? dataStream = null)
        {
            dataStream = (globalMessageEncryptor!=null && messageEncryptor is NonEncryptor<T> ? globalMessageEncryptor : messageEncryptor).Decrypt(dataStream??new MemoryStream(message.Data.ToArray()), message.Header);
            object? result = (globalMessageEncoder!=null && messageEncoder is JsonEncoder<T> ? globalMessageEncoder.Decode<T>(dataStream) : messageEncoder.Decode(dataStream));
            foreach (var converter in path)
            {
                logger?.LogTrace("Attempting to convert {SourceType} to {DestiniationType} through converters for {IntermediateType}", Utility.TypeName<T>(), Utility.TypeName<V>(), Utility.TypeName(ExtractGenericArguements(converter.GetType())[0]));
                result = ExecuteConverter(converter, result, ExtractGenericArguements(converter.GetType())[1]);
            }
            return (V?)result;
        }

        private static Type[] ExtractGenericArguements(Type t) => t.GetInterfaces().First(iface => iface.IsGenericType && iface.GetGenericTypeDefinition()==typeof(IMessageConverter<,>)).GetGenericArguments();

        private static object? ExecuteConverter(object converter, object? source, Type destination)
        {
            if (source==null)
                return null;
            return typeof(IMessageConverter<,>).MakeGenericType(source.GetType(), destination)
                .GetMethod("Convert")!
                .Invoke(converter, [source]);
        }
    }
}
