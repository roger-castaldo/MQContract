using MQContract.Attributes;

namespace BenchMark.Messages
{
    [Message(typeName:"EncodedAnnouncement",typeVersion:"1.0.0")]
    internal record EncodedAnnouncement(string Message) : Announcement(Message)
    {
    }
}
