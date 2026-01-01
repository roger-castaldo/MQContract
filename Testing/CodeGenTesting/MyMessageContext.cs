using CodeGenTesting.Converters;
using CodeGenTesting.Encoders;
using CodeGenTesting.Messages;
using MQContract;
using MQContract.Attributes;

namespace CodeGenTesting
{
    [UseMqContract(typeof(Announcement))]
    [UseMqContract(typeof(PartyAnnouncement),encoderType: typeof(PartyAnnouncementEncoder), converters: [typeof(AnnouncementConverter)])]
    [UseMqContract(typeof(DirectAnnouncement), encoderType: typeof(DirectAnnouncementEncoder), converters: [typeof(AnnouncementConverter)])]
    internal partial class MyMessageContext : MQContractMessageContext
    {
    }
}
