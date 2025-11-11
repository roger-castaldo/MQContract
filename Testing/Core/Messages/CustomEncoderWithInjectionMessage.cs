using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [Message(channel: "CustomEncoderWithInjection")]
    public record CustomEncoderWithInjectionMessage(string TestName) { }
}
