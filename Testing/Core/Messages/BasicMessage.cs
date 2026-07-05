using MQContract.Attributes;

namespace CoreTesting.Messages
{
    [Message(channel: "BasicMessage")]
    public record BasicMessage(string Name);
}
