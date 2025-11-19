using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Messages;
using System.Reflection;

namespace MQContract.Factories
{
    internal abstract class AConverter<TMeessageType,TSourceMessageType>
        : IConversionPath<TMeessageType>
    {
        private readonly string messageTypeName = $"{Utility.GetCustomAttribute<TSourceMessageType,MessageAttribute>()?.TypeName??Utility.TypeName<TSourceMessageType>()}-{typeof(TSourceMessageType).GetCustomAttribute<MessageAttribute>()?.TypeVersion.ToString()??"0.0.0.0"}";

        protected abstract ValueTask<TMeessageType?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream);

        ValueTask<TMeessageType?> IConversionPath<TMeessageType>.ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream)
            => ConvertMessageAsync(logger, message, dataStream);

        bool IConversionPath<TMeessageType>.IsMatch(string metaData)
            => messageTypeName.Equals(metaData, StringComparison.InvariantCulture);
    }
}
