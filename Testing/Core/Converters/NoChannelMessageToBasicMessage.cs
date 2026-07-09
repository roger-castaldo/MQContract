using CoreTesting.Messages;
using MQContract.Interfaces.Conversion;

namespace CoreTesting.Converters;

internal class NoChannelMessageToBasicMessage : IMessageConverter<NoChannelMessage, BasicMessage>
{
    public ValueTask<BasicMessage> ConvertAsync(NoChannelMessage source)
        => ValueTask.FromResult<BasicMessage>(new(source.TestName));
}
