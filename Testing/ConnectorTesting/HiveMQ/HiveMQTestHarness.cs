using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace ConnectorTesting.Harnesses;

internal class HiveMQTestHarness : AServiceHarness
{
    public string HostName { get; private set; } = string.Empty;
    public int HostPort { get; private set; }

    protected override IContainer Build()
        => new ContainerBuilder("hivemq/hivemq-ce:latest")
        .WithPortBinding(1883, true) // Bind container port 1883 to a random host port
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Started HiveMQ in"))
            .Build();

    protected override void PostStart(IContainer container)
    {
        HostName = container.Hostname;
        HostPort = container.GetMappedPublicPort(1883);
    }
}
