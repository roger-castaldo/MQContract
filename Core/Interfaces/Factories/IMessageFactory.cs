using MQContract.Interfaces.Conversion;
using MQContract.Messages;

namespace MQContract.Interfaces.Factories
{
    internal interface IMessageFactory<TMessage> : IMessageTypeFactory, IConversionPath<TMessage>
    {
        string? MessageChannel { get; }
        ValueTask<ServiceMessage> ConvertMessageAsync(TMessage message, bool ignoreChannel, string? channel, MessageHeader messageHeader);
    }
}
