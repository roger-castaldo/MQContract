using AutomatedTesting.Messages;
using MQContract.Interfaces.Conversion;

namespace AutomatedTesting.Converters
{
    internal class NoChannelMessageToBasicMessage : IMessageConverter<NoChannelMessage, BasicMessage>
    {
        public BasicMessage Convert(NoChannelMessage source)
            => new(source.TestName);
    }
}
