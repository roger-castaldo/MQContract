using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("CustomEncryptorWithInjection")]
    public record CustomEncryptorWithInjectionMessage(string TestName) { }
}
