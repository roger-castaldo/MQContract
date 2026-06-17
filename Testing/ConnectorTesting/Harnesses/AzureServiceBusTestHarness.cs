using Testcontainers.ServiceBus;

namespace ConnectorTesting.Harnesses;

internal class AzureServiceBusTestHarness : IAsyncDisposable
{
    private readonly ServiceBusContainer container = new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
        .WithAcceptLicenseAgreement(true)
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task StartAsync()
    {
        await container.StartAsync();
        ConnectionString = container.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        await container.StopAsync();
    }
}
