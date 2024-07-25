using MQContract.Attributes;

namespace Messages
{
    [MessageChannel("StoredArrivals")]
    public record StoredArrivalAnnouncement(string FirstName,string LastName){}
}
