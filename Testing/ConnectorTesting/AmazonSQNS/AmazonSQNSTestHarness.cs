using DotNet.Testcontainers.Containers;
using Testcontainers.LocalStack;

namespace ConnectorTesting.Harnesses;

internal class AmazonSQNSTestHarness : AServiceHarness
{
    public string ConnectionString { get; private set; } = string.Empty;

    protected override IContainer Build()
        => new LocalStackBuilder("localstack/localstack:4.14")
        .WithEnvironment("SERVICES", "sqs,sns")
        .Build();

    protected override void PostStart(IContainer container)
    {
        ConnectionString = container.GetConnectionString();
    }
}
