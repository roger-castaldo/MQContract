using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Messages;

namespace MQContract.Interfaces.Conversion
{
    internal interface IConversionPath<T>
        where T : class
    {
        ValueTask<T?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message, Stream? dataStream = null);
    }
}
