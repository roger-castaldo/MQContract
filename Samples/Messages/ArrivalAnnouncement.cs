using MQContract.Attributes;

namespace Messages
{
    [Message()]
    public record ArrivalAnnouncement(string FirstName, string LastName) { }
}
