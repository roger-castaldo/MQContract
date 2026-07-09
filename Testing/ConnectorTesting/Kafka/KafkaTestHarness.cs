using DotNet.Testcontainers.Containers;
using Testcontainers.Kafka;

namespace ConnectorTesting.Harnesses;

internal class KafkaTestHarness : AServiceHarness
{
    public string BootstrapAddress { get; private set; } = string.Empty;

    // Use a Confluent Platform 6.2.x image to avoid the newer KAFKA_PROCESS_ROLES requirement
    protected override IContainer Build()
        => new KafkaBuilder("confluentinc/cp-kafka:6.2.1")
            .Build();

    protected override void PostStart(IContainer container)
    {
        BootstrapAddress = ((KafkaContainer)container).GetBootstrapAddress();
    }
}
