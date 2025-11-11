using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [Message(channel: "CustomEncryptorWithInjection")]
    public record CustomEncryptorWithInjectionMessage(string TestName) { }
}
