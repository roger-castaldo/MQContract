using DotNet.Testcontainers.Containers;
using Testcontainers.Pulsar;

namespace ConnectorTesting.Harnesses;

internal class PulsarTestHarness : AServiceHarness
{
    public string BrokerAddress { get; private set; } = string.Empty;

    protected override IContainer Build()
        => new PulsarBuilder("apachepulsar/pulsar:3.0.9")
            .Build();

    protected override void PostStart(IContainer container)
    {
        BrokerAddress = ((PulsarContainer)container).GetBrokerAddress();
    }
}
