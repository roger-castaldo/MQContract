using MQContract.CQRS.Attributes;
using MQContract.CQRS.Interfaces.Command;

namespace CQRSTesting.Messages
{
    [Command("BasicResponseCommand")]
    public record BasicResponseCommand(string Name) : ICommand<BasicCommandResponse>
    {
    }
}
