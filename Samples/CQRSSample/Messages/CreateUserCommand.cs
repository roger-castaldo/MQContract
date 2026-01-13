using MQContract.CQRS.Attributes;
using MQContract.CQRS.Interfaces.Command;

namespace Messages
{
    [Command(channel: "UserCommands", typeName: "CreateUser", typeVersion: "1.0.0")]
    public record CreateUserCommand(string UserId, string UserName, string Email) : ICommand;
}