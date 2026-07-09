using MQContract.Attributes;

namespace CoreTesting.Messages;

[Message(channel: "CustomEncoderWithInjection")]
public record CustomEncoderWithInjectionMessage(string TestName) { }
