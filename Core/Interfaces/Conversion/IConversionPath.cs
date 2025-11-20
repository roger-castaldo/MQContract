using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Messages;

namespace MQContract.Interfaces.Conversion
{
    internal interface IConversionPath<TMessage>
    {
        bool IsMatch(string metaData);
        ValueTask<TMessage?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream = null);
    }
}
