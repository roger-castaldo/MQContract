using MQContract.Interfaces.Conversion;
using MQContract.Messages;

namespace MQContract.Interfaces.Factories
{
    internal interface IMessageFactory<T> : IMessageTypeFactory, IConversionPath<T> where T : class
    {
        string? MessageChannel { get; }
        ValueTask<ServiceMessage> ConvertMessageAsync(T message,bool ignoreChannel, string? channel, MessageHeader messageHeader);
    }
}
