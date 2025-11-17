using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [Message(channel: "BasicMessage")]
    public record BasicMessage(string Name);
}
