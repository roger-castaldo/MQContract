using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace Messages;

internal class AnnouncementConsumer : IPubSubConsumer<ArrivalAnnouncement>
{
    void IBaseConsumer.ErrorRecieved(Exception error)
    {
        Console.WriteLine($"Announcement error: {error.Message}");
    }

    void IPubSubConsumer<ArrivalAnnouncement>.MessageReceived(IReceivedMessage<ArrivalAnnouncement> message)
    {
        Console.WriteLine($"Announcing the arrival of {message.Message.LastName}, {message.Message.FirstName} in member 2 of the group.. [{message.ID},{message.ReceivedTimestamp}]");
    }
}
