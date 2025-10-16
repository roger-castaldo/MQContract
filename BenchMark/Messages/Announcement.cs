using MQContract.Attributes;

namespace BenchMark.Messages
{
    [MessageName("Announcement")]
    [MessageVersion("1.0.0")]
    internal record Announcement(string Message)
    { }
}
