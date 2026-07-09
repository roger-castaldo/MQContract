using DotNet.Testcontainers.Containers;
using Testcontainers.Redis;

namespace ConnectorTesting.Harnesses;

internal class RedisTestHarness : AServiceHarness
{
    public string Endpoints { get; private set; } = default!;

    protected override IContainer Build()
        => new RedisBuilder("redis:latest")
            .Build();

    protected override void PostStart(IContainer container)
    {
        Endpoints = container.GetConnectionString();
    }
}
