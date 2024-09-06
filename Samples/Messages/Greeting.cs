using MQContract.Attributes;

namespace Messages
{
    [MessageChannel("Greeting")]
    [MessageName("Nametag")]
    [MessageVersion("1.0.0.0")]
    [QueryResponseType(typeof(string))]
    [QueryResponseChannel("Greeting.Response")]
    public record Greeting(string FirstName,string LastName){}
}
