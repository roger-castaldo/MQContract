using MQContract.Attributes;

namespace CodeGenTesting.Messages
{
    [QueryMessage("NonContextPrompt", responseChannel: "NonContextPromptResponse", responseTimeoutMilliseconds:500,responseType:typeof(NonContextReply))]
    public record NonContextPrompt(string FirstName, string LastName)
    {}

    public record NonContextReply(string Greeting)
    { }
}
