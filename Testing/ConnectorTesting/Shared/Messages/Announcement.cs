using MQContract.Attributes;

namespace ConnectorTesting.Messages;

[Message("Announcement", typeName: "Announcement", typeVersion: "1.0.0")]
public record Announcement(string Message)
{ }