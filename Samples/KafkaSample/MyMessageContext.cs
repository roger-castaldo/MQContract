using Messages;
using MQContract;
using MQContract.Attributes;

namespace KafkaSample;

[UseMqContract(typeof(ArrivalAnnouncement))]
[UseMqContract(typeof(Greeting))]
[UseMqContract(typeof(StoredArrivalAnnouncement))]
internal partial class MyMessageContext : MQContractMessageContext
{
}
