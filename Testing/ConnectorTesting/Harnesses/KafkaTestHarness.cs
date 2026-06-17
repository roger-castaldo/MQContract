using Testcontainers.Kafka;

namespace ConnectorTesting.Harnesses;

internal class KafkaTestHarness : IAsyncDisposable
{
    // Use a Confluent Platform 6.2.x image to avoid the newer KAFKA_PROCESS_ROLES requirement
    private readonly KafkaContainer container = new KafkaBuilder("confluentinc/cp-kafka:6.2.1")
            .Build();

    public string BootstrapAddress { get; private set; } = string.Empty;

    public async Task StartAsync()
    {
        await container.StartAsync();
        BootstrapAddress = container.GetBootstrapAddress();
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
    }
}
