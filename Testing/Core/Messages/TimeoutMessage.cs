using MQContract.Attributes;

namespace CoreTesting.Messages;

[QueryMessage(channel: "Timeout", responseTimeoutMilliseconds: 500)]
public record TimeoutMessage(string Name) { }
