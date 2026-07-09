
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using System.Reflection;
using Testcontainers.MsSql;
using Testcontainers.ServiceBus;

namespace ConnectorTesting.Harnesses;

internal class AzureServiceBusTestHarness : IAsyncDisposable
{
    private const string SqlAlias = "mssql";

    public string ConnectionString { get; private set; } = string.Empty;
    public string AdminConnectionString { get; private set; } = string.Empty;

    private readonly INetwork network;
    private readonly MsSqlContainer sqlContainer;
    private readonly ServiceBusContainer serviceBusContainer;

    public AzureServiceBusTestHarness()
    {
        network = new NetworkBuilder()
            .Build();
        sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
            .WithNetwork(network)
            .WithNetworkAliases(SqlAlias)
            .Build();
        serviceBusContainer = new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
            .WithMsSqlContainer(network, sqlContainer, SqlAlias)
            .WithAcceptLicenseAgreement(true)
            .WithReuse(false)
            .Build();
    }

    public async Task StartAsync()
    {
        await network.CreateAsync();
        await sqlContainer.StartAsync();
        await serviceBusContainer.StartAsync();

        // Attempt to reduce resource usage of the running container by updating
        // its HostConfig via the Docker Engine Update API. We use reflection to
        // retrieve the underlying container id from the Testcontainers IContainer
        // implementation because the interface does not expose the id directly.
        try
        {
            UpdateContainerResources(sqlContainer, memory: 512 * 1024 * 1024, nanoCpus: 1_000_000_000);
        }
        catch
        {
            // Best-effort: if we cannot update resources, do not fail the harness startup.
        }

        ConnectionString = serviceBusContainer.GetConnectionString();
        AdminConnectionString = $"Endpoint=sb://{serviceBusContainer.Hostname}:{serviceBusContainer.GetMappedPublicPort(5300)};SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
    }

    private static void UpdateContainerResources(IContainer container, long memory, long nanoCpus)
    {
        // Discover likely property names for the container id on the implementation
        var type = container.GetType();
        var idProp = type.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                 ?? type.GetProperty("ContainerId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                 ?? type.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        var id = idProp?.GetValue(container) as string;
        if (string.IsNullOrWhiteSpace(id))
            return;

        // Attempt to call the Docker CLI to update resource limits. This avoids a hard dependency
        // on Docker.DotNet types at runtime and works in CI where the docker CLI is available.
        try
        {
            var memoryMb = memory / (1024 * 1024);
            var cpus = nanoCpus / 1_000_000_000.0; // e.g., 1.0

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"update --memory {memoryMb}m --cpus {cpus.ToString(System.Globalization.CultureInfo.InvariantCulture)} {id}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc != null)
            {
                proc.WaitForExit(10_000); // wait up to 10s
            }
        }
        catch
        {
            // Ignore any errors; this is a best-effort attempt.
        }
    }

    public async ValueTask DisposeAsync()
    {
        await serviceBusContainer.StopAsync().ConfigureAwait(true);
        await sqlContainer.StopAsync().ConfigureAwait(true);

        await serviceBusContainer.DisposeAsync().ConfigureAwait(true);
        await sqlContainer.DisposeAsync().ConfigureAwait(true);
        await network.DisposeAsync().ConfigureAwait(true);
    }
}
