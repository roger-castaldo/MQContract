using BenchMark.Encoders;
using BenchMark.Messages;
using MQContract;
using MQContract.Attributes;

namespace BenchMark;

[UseMqContractAttribute(typeof(Announcement))]
[UseMqContractAttribute(typeof(AnnouncementCommand))]
[UseMqContractAttribute(typeof(EncodedAnnouncement), encoderType: typeof(EncodedAnnouncementEncoder))]
internal partial class MyMessageContext : MQContractMessageContext
{
}
