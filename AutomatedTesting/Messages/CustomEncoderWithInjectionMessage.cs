using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("CustomEncoderWithInjection")]
    public record CustomEncoderWithInjectionMessage(string TestName) { }
}
