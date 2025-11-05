using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [Message(channel: "CustomEncoder")]
    public record CustomEncoderMessage(string TestName) { }
}
