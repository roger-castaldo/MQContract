using MQContract.Interfaces.Conversion;
using MQContract.ServiceAbstractions.Messages;

namespace MQContract.Interfaces.Factories
{
    internal interface IMessageFactory<T> : IMessageTypeFactory, IConversionPath<T> where T : class
    {
        IServiceMessage ConvertMessage(T message, string? channel, IMessageHeader? messageHeader = null);
    }
}
