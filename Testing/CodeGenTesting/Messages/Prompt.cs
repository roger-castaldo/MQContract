using MQContract.Attributes;

namespace CodeGenTesting.Messages
{
    [QueryMessage("Prompt", responseChannel: "PromptResponse", responseTimeoutMilliseconds: 500, responseType: typeof(Reply))]
    public record Prompt(string FirstName, string LastName)
    { }

    public record Reply(string Greeting)
    { }
}
