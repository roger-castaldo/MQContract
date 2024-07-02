using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("CustomEncoder")]
    public record CustomEncoderMessage(string TestName) { }
}
