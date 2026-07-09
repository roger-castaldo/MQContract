using MQContract.Attributes;

namespace CoreTesting.Messages;

[Message(channel: "NamedAndVersioned", typeName: "VersionedMessage", typeVersion: "1.0.0.3")]
public record NamedAndVersionedMessage(string TestName) { }
