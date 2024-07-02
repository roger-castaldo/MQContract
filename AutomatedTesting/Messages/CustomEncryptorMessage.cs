using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("CustomEncryptorMessage")]
    public record CustomEncryptorMessage(string TestName) { }
}
