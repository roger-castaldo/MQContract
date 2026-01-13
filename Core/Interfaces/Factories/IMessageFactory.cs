using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Messages;
using MQContract.Messages;

namespace MQContract.Interfaces.Factories
{
    internal interface IMessageFactory<TMessage> : IMessageTypeFactory
    {
        string? MessageChannel { get; }
        ValueTask<ServiceMessage> ConvertMessageAsync(TMessage message, bool ignoreChannel, string? channel, MessageHeader messageHeader);
        ValueTask<TMessage?> ConvertMessageAsync(ILogger? logger, IEncodedMessage message);
    }
}
