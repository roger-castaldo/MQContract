using CodeGenTesting.Messages;
using MQContract;
using MQContract.Attributes;

namespace CodeGenTesting;

[MQContractMessageContext(true, true, true)]
[UseMqContract(typeof(Announcement))]
[UseMqContract(typeof(PartyAnnouncement))]
[UseMqContract(typeof(DirectAnnouncement))]
[UseMqContract(typeof(Prompt))]
internal partial class MyAutoMessageContext : MQContractMessageContext
{
}
