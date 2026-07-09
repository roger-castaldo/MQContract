using MQContract.Attributes;

namespace CoreTesting.Messages;

[Message(channel: "CustomEncryptorMessage")]
public record CustomEncryptorMessage(string TestName) { }
