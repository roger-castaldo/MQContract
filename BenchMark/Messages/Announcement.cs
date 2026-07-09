using MQContract.Attributes;

namespace BenchMark.Messages;

[Message(typeName: "Announcement", typeVersion: "1.0.0")]
public record Announcement(string Message)
{ }
