using MQContract.CQRS.Attributes;
using MQContract.CQRS.Interfaces.Command;

namespace BenchMark.Messages
{
    [Command(channel: "Announcements")]
    public record AnnouncementCommand(string Message) : ICommand
    { }
}
