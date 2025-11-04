using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Messages;
using System.Reflection;

namespace MQContract.Factories
{
    internal abstract class AConverter<T,M>
        : IConversionPath<T>
    {
        private readonly string messageTypeName = $"{typeof(M).GetCustomAttribute<MessageNameAttribute>()?.Value??Utility.TypeName<M>()}-{typeof(M).GetCustomAttribute<MessageVersionAttribute>()?.Version.ToString()??"0.0.0.0"}";

        protected abstract ValueTask<T?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream);

        ValueTask<T?> IConversionPath<T>.ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream)
            => ConvertMessageAsync(logger, message, dataStream);

        bool IConversionPath<T>.IsMatch(string metaData)
            => messageTypeName.Equals(metaData, StringComparison.InvariantCulture);
    }
}
