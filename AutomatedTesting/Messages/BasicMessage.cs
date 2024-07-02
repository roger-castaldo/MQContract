using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("BasicMessage")]
    public record BasicMessage(string Name);
}
