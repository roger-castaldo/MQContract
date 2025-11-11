using AutomatedTesting.Messages;
using MQContract.Interfaces.Conversion;

namespace AutomatedTesting.Converters
{
    internal class NoChannelMessageToBasicMessage : IMessageConverter<NoChannelMessage, BasicMessage>
    {
        public ValueTask<BasicMessage> ConvertAsync(NoChannelMessage source)
            => ValueTask.FromResult<BasicMessage>(new(source.TestName));
    }
}
