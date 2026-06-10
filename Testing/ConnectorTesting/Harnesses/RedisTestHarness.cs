using NATS.Client.Core;
using Testcontainers.Nats;
using Testcontainers.Redis;

namespace ConnectorTesting.Harnesses;

internal class RedisTestHarness : IAsyncDisposable
{
    private readonly RedisContainer container = new RedisBuilder("redis:latest")
            .Build();

    public string Endpoints { get; private set; } = default!;

    public async Task StartAsync()
    {
        await container.StartAsync();
        Endpoints = container.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
    }
}
