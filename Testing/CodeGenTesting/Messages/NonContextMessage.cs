using MQContract.Attributes;

namespace CodeGenTesting.Messages
{
    [Message(typeName: "NotAContextMessage", typeVersion: "12.0.0.0")]
    internal record NonContextMessage(string Message)
    { }
}
