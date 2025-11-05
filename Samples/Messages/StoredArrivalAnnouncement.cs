using MQContract.Attributes;

namespace Messages
{
    [Message()]
    public record StoredArrivalAnnouncement(string FirstName, string LastName) { }
}
