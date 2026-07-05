using MQContract.Attributes;

namespace CoreTesting.Messages
{
    [Message(channel: "CustomEncoder")]
    public record CustomEncoderMessage(string TestName) { }
}
