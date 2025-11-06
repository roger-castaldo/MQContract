using MQContract.Attributes;

namespace Messages
{
    [Message(channel: "Arrivals")]
    public record ArrivalAnnouncement(string FirstName, string LastName) { }
}
