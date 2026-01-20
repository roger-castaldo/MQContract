using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [QueryMessage(channel: "Timeout", responseTimeoutMilliseconds: 500)]
    public record TimeoutMessage(string Name) { }
}
