using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("NamedAndVersioned")]
    [MessageName("VersionedMessage")]
    [MessageVersion("1.0.0.3")]
    public record NamedAndVersionedMessage(string TestName) { }
}
