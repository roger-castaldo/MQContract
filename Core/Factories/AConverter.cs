using Microsoft.Extensions.Logging;
using MQContract.Helpers;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Messages;

namespace MQContract.Factories
{
    internal abstract class AConverter<TMeessageType,TSourceMessageType>
        : IConversionPath<TMeessageType>
    {
        private readonly string messageTypeName = $"{MessageTypeHelper.MessageTypeName<TSourceMessageType>()}-{MessageTypeHelper.MessageVersionString<TSourceMessageType>()}";

        protected abstract ValueTask<TMeessageType?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream);

        ValueTask<TMeessageType?> IConversionPath<TMeessageType>.ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream)
            => ConvertMessageAsync(logger, message, dataStream);

        bool IConversionPath<TMeessageType>.IsMatch(string metaData)
            => messageTypeName.Equals(metaData, StringComparison.InvariantCulture);
    }
}
