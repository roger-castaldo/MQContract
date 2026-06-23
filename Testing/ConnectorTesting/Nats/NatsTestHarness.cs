using DotNet.Testcontainers.Containers;
using NATS.Client.Core;
using Testcontainers.Nats;

namespace ConnectorTesting.Harnesses;

internal class NatsTestHarness : AServiceHarness
{
    public NatsOpts Options { get; private set; } = default!;

    protected override IContainer Build()
        => new NatsBuilder("nats:latest")
            .Build();

    protected override void PostStart(IContainer container)
    {
        Options = new()
        {
            Url = container.GetConnectionString()
        };
    }
}
