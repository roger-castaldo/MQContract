using MQContract.Attributes;

namespace Messages;

[Message(channel: "StoredArrivals")]
public record StoredArrivalAnnouncement(string FirstName, string LastName) { }
