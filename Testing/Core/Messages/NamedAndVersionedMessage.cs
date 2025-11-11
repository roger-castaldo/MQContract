using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [Message(channel: "NamedAndVersioned",typeName:"VersionedMessage",typeVersion:"1.0.0.3")]
    public record NamedAndVersionedMessage(string TestName) { }
}
