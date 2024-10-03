using MQContract.Attributes;

namespace AutomatedTesting.Messages
{
    [MessageChannel("BasicQueryMessage")]
    [QueryResponseType(typeof(BasicResponseMessage))]
    [QueryResponseChannel("BasicQueryResponse")]
    public record BasicQueryMessage(string TypeName) { }
}
