using MQContract.Attributes;

namespace Messages
{
    [MessageChannel("Arrivals")]
    public record ArrivalAnnouncement(string FirstName,string LastName){}
}
