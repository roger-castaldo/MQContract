using Testcontainers.RabbitMq;

namespace ConnectorTesting.Harnesses;

internal class RabbitMQTestHarness : IAsyncDisposable
{
    // Use a Confluent Platform 6.2.x image to avoid the newer KAFKA_PROCESS_ROLES requirement
    private readonly RabbitMqContainer container = new RabbitMqBuilder("rabbitmq:3-management")
            .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task StartAsync()
    {
        await container.StartAsync();
        ConnectionString = container.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
    }
}
