using ConnectorTesting.Helpers;
using Testcontainers.ActiveMq;
using DotNet.Testcontainers.Containers;

namespace ConnectorTesting.Harnesses;

internal class ActiveMQTestHarness : AServiceHarness
{
    private static readonly string username = TestHelper.RandomString(10);

    private static readonly string password = TestHelper.RandomString(10);

    protected override IContainer Build()
        => new ArtemisBuilder("apache/activemq-artemis:latest")
        .WithUsername(username)
        .WithPassword(password)
            .Build();

    protected override void PostStart(IContainer container)
    {
        var artemisUri = new Uri(((ArtemisContainer)container).GetBrokerAddress());
        BrokerAddress = $"amqp://{artemisUri.Host}:{artemisUri.Port}";
    }

    public string BrokerAddress { get; private set; } = default!;
    public string UserName => username;
    public string Password => password;
}
