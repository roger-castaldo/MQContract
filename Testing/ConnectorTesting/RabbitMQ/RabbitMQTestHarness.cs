using DotNet.Testcontainers.Containers;
using Testcontainers.RabbitMq;

namespace ConnectorTesting.Harnesses;

internal class RabbitMQTestHarness : AServiceHarness
{
    public string ConnectionString { get; private set; } = string.Empty;

    protected override IContainer Build()
        => new RabbitMqBuilder("rabbitmq:3-management")
            .Build();

    protected override void PostStart(IContainer container)
    {
        ConnectionString = container.GetConnectionString();
    }
}
