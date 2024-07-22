using AutomatedTesting.Messages;
using MQContract.Interfaces.Conversion;

namespace AutomatedTesting.Converters
{
    internal class BasicMessageToNameAndVersionMessage : IMessageConverter<BasicMessage, NamedAndVersionedMessage>
    {
        public NamedAndVersionedMessage Convert(BasicMessage source)
            => new(source.Name);
    }
}
