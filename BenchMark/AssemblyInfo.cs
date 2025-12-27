using BenchMark.Messages;
using MQContract.Attributes;

[assembly: UseMqContractAttribute(typeof(Announcement))]
[assembly: UseMqContractAttribute(typeof(AnnouncementCommand))]
[assembly: UseMqContractAttribute(typeof(EncodedAnnouncement))]