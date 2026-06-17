using Testcontainers.Pulsar;

namespace ConnectorTesting.Harnesses;

internal class PulsarTestHarness : IAsyncDisposable
{
    private readonly PulsarContainer container = new PulsarBuilder("apachepulsar/pulsar:3.0.9")
            .Build();

    public string BrokerAddress { get; private set; } = string.Empty;

    public async Task StartAsync()
    {
        await container.StartAsync();
        BrokerAddress = container.GetBrokerAddress();
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
    }
}
