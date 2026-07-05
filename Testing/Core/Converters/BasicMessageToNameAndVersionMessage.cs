using CoreTesting.Messages;
using MQContract.Interfaces.Conversion;

namespace CoreTesting.Converters
{
    internal class BasicMessageToNameAndVersionMessage :
        IMessageConverter<BasicMessage, NamedAndVersionedMessage>
    {
        public ValueTask<NamedAndVersionedMessage> ConvertAsync(BasicMessage source)
            => ValueTask.FromResult<NamedAndVersionedMessage>(new(source.Name));
    }
}
