using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Defaults;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Messages;

namespace MQContract.Factories
{
    internal class ConversionPath<T, V> : AConverter<V,T>
        where T : class
        where V : class
    {
        private readonly IEnumerable<object> path;
        private readonly IMessageTypeEncoder<T> messageEncoder;
        private readonly IMessageEncoder? globalMessageEncoder;

        public ConversionPath(IEnumerable<object> path, IEnumerable<Type> types, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            this.path = path;
            this.globalMessageEncoder = globalMessageEncoder;
            var encoderType = types
                .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IMessageTypeEncoder<T>)), typeof(JsonEncoder<T>));
            messageEncoder = (IMessageTypeEncoder<T>)(serviceProvider!=null ? ActivatorUtilities.CreateInstance(serviceProvider, encoderType) : Activator.CreateInstance(encoderType)!);
        }

        protected override async ValueTask<V?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream)
        {
            dataStream ??= new MemoryStream(message.Data.ToArray());
            object? result = await (globalMessageEncoder!=null && messageEncoder is JsonEncoder<T> ? globalMessageEncoder.DecodeAsync<T>(dataStream) : messageEncoder.DecodeAsync(dataStream));
            foreach (var converter in path)
            {
                logger?.LogTrace("Attempting to convert {SourceType} to {DestiniationType} through converters for {IntermediateType}", Utility.TypeName<T>(), Utility.TypeName<V>(), Utility.TypeName(ExtractGenericArguements(converter.GetType())[0]));
                result = await ExecuteConverter(converter, result, ExtractGenericArguements(converter.GetType())[1]);
            }
            return (V?)result;
        }

        private static Type[] ExtractGenericArguements(Type t) => t.GetInterfaces().First(iface => iface.IsGenericType && iface.GetGenericTypeDefinition()==typeof(IMessageConverter<,>)).GetGenericArguments();

        private static async ValueTask<object?> ExecuteConverter(object converter, object? source, Type destination)
        {
            if (source==null)
                return null;
            return await Utility.InvokeMethodAsync(
                typeof(IMessageConverter<,>).MakeGenericType(source.GetType(), destination)
                .GetMethod("ConvertAsync")!,
                converter,
                [source]
            );
        }
    }
}
