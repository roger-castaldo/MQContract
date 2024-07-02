using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("BasicQueryMessage")]
    [QueryResponseType(typeof(BasicResponseMessage))]
    public record BasicQueryMessage(string TypeName) { }
}
