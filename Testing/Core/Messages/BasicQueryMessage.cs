using MQContract.Attributes;

namespace CoreTesting.Messages
{
    [QueryMessage(channel: "BasicQueryMessage", responseType: typeof(BasicResponseMessage), responseChannel: "BasicQueryResponse")]
    public record BasicQueryMessage(string TypeName) { }
}
