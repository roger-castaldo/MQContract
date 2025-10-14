using MQContract.Attributes;

namespace BenchMark.Messages
{
    [MessageName("EncodedAnnouncement")]
    [MessageVersion("1.0.0")]
    internal record EncodedAnnouncement(string Message) : Announcement(Message)
    {
    }
}
