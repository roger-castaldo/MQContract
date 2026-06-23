using MQContract;
using MQContract.Attributes;

namespace ConnectorTesting.Messages;

[MQContractMessageContext(true, true, true)]
[UseMqContract(typeof(Announcement))]
[UseMqContract(typeof(Prompt))]
internal partial class TestMessageContext : MQContractMessageContext
{
}
