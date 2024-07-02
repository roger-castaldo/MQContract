using Microsoft.Extensions.Logging;
using MQContract.ServiceAbstractions.Messages;

namespace MQContract.Interfaces.Conversion
{
    internal interface IConversionPath<out T> : IBaseConversionPath
        where T : class
    {
        T? ConvertMessage(ILogger? logger, IServiceMessage message, Stream? dataStream = null);
    }
}
