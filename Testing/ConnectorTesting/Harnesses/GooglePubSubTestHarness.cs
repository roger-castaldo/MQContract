using Testcontainers.PubSub;

namespace ConnectorTesting.Harnesses;

internal class GooglePubSubTestHarness : IAsyncDisposable
{
    // Use a Confluent Platform 6.2.x image to avoid the newer KAFKA_PROCESS_ROLES requirement
    private readonly PubSubContainer container = new PubSubBuilder("gcr.io/google.com/cloudsdktool/google-cloud-cli:446.0.1-emulators")
            .Build();

    public string EndPoint { get; private set; } = string.Empty;

    public async Task StartAsync()
    {
        await container.StartAsync();
        EndPoint = container.GetEmulatorEndpoint();
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
    }
}
