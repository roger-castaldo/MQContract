using MQContract.CQRS.Attributes;
using MQContract.CQRS.Interfaces.Command;

namespace CQRSTesting.Messages;

[Command("BasicCommand")]
public record BasicCommand(string Name) : ICommand
{
}
