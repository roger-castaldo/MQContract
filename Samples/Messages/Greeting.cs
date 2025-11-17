using MQContract.Attributes;

namespace Messages
{
    [QueryMessage(channel: "Greeting",typeName:"Nametag",typeVersion:"1.0.0.0",responseType:typeof(string),responseChannel:"Greeting.Response")]
    public record Greeting(string FirstName, string LastName) { }
}
