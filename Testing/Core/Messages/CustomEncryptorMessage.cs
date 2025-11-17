using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [Message(channel: "CustomEncryptorMessage")]
    public record CustomEncryptorMessage(string TestName) { }
}
