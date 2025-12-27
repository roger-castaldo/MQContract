using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Defaults;
using MQContract.Extensions;
using MQContract.Helpers;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Messages;

namespace MQContract.Factories
{
    internal class ConversionPath<TMessage, TResult> : AConverter<TResult,TMessage>
        where TMessage : class
        where TResult : class
    {
        private readonly IEnumerable<object> path;
        private readonly IMessageTypeEncoder<TMessage> messageEncoder;
        private readonly IMessageEncoder? globalMessageEncoder;

        public ConversionPath(IEnumerable<object> path, IEnumerable<Type> types, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider)
        {
            this.path = path;
            this.globalMessageEncoder = globalMessageEncoder;
            var encoderType = types
                .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IMessageTypeEncoder<TMessage>)), typeof(JsonEncoder<TMessage>));
            messageEncoder = (IMessageTypeEncoder<TMessage>)(serviceProvider!=null ? ActivatorUtilities.CreateInstance(serviceProvider, encoderType) : Activator.CreateInstance(encoderType)!);
        }

        protected override async ValueTask<TResult?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream)
        {
            dataStream ??= new MemoryStream(message.Data.ToArray(),0,message.Data.Length,false,true);
            object? result = await (globalMessageEncoder!=null && messageEncoder is JsonEncoder<TMessage> ? globalMessageEncoder.DecodeAsync<TMessage>(dataStream) : messageEncoder.DecodeAsync(dataStream));
            foreach (var converter in path)
            {
                logger?.LogTraceChecked("Attempting to convert {SourceType} to {DestiniationType} through converters for {IntermediateType}", MessageTypeHelper.MessageTypeName<TMessage>(), MessageTypeHelper.MessageTypeName<TResult>(), MessageTypeHelper.MessageTypeName(ExtractGenericArguements(converter.GetType())[0]));
                result = await ExecuteConverter(converter, result, ExtractGenericArguements(converter.GetType())[1]);
            }
            return (TResult?)result;
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
