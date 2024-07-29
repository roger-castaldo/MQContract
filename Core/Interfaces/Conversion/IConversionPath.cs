using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Messages;
using MQContract.Messages;

namespace MQContract.Interfaces.Conversion
{
    internal interface IConversionPath<out T>
        where T : class
    {
        T? ConvertMessage(ILogger? logger, IEncodedMessage message, Stream? dataStream = null);
    }
}
