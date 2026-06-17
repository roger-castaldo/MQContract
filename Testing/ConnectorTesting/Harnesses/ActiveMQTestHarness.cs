using ConnectorTesting.Helpers;
using Testcontainers.ActiveMq;

namespace ConnectorTesting.Harnesses;

internal class ActiveMQTestHarness : IAsyncDisposable
{
    private static readonly string username = TestHelper.RandomString(10);

    private static readonly string password = TestHelper.RandomString(10);

    private readonly ArtemisContainer container = new ArtemisBuilder("apache/activemq-artemis:latest")
        .WithUsername(username)
        .WithPassword(password)
            .Build();

    public string BrokerAddress { get; private set; } = default!;
    public string UserName => username;
    public string Password => password;

    public async Task StartAsync()
    {
        await container.StartAsync();
        var artemisUri = new Uri(container.GetBrokerAddress());
        BrokerAddress = $"amqp://{artemisUri.Host}:{artemisUri.Port}";
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
    }
}
