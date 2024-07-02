using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("Timeout")]
    [MessageResponseTimeout(500)]
    public record TimeoutMessage(string Name) { }
}
