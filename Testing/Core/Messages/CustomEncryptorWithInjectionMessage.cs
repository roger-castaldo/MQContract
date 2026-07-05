using MQContract.Attributes;

namespace CoreTesting.Messages
{
    [Message(channel: "CustomEncryptorWithInjection")]
    public record CustomEncryptorWithInjectionMessage(string TestName) { }
}
