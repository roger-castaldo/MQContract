using ConnectorTesting.Helpers;
using Testcontainers.ActiveMq;

namespace ConnectorTesting.Harnesses;

internal class ActiveMQTestHarness : IAsyncDisposable
{
    private static readonly string username = TestHelper.RandomString(10);

    private static readonly string password = TestHelper.RandomString(10);

    private readonly ArtemisContainer container = new ArtemisBuilder("apache/activemq:latest")
        .WithUsername(username)
        .WithPassword(password)
            .Build();

    public string BrokerAddress { get; private set; } = default!;

    public async Task StartAsync()
    {
        await container.StartAsync();
        BrokerAddress = container.GetBrokerAddress();
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
    }
}
