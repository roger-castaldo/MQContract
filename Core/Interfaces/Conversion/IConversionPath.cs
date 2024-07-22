using Microsoft.Extensions.Logging;
using MQContract.Messages;

namespace MQContract.Interfaces.Conversion
{
    internal interface IConversionPath<out T>
        where T : class
    {
        T? ConvertMessage(ILogger? logger, IServiceMessage message, Stream? dataStream = null);
    }
}
