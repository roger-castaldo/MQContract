using DotNet.Testcontainers.Containers;

namespace ConnectorTesting.Harnesses;

internal abstract class AServiceHarness : IAsyncDisposable
{
    private IContainer? container;

    protected abstract IContainer Build();
    protected abstract void PostStart(IContainer container);

    public async Task StartAsync()
    {
        container = Build();
        await container.StartAsync();
        PostStart(container);
    }

    public async ValueTask DisposeAsync()
    {
        await (container?.StopAsync()??Task.CompletedTask).ConfigureAwait(true);
        await Task.Delay(TimeSpan.FromSeconds(30));
        await (container?.DisposeAsync()??ValueTask.CompletedTask);
    }
}
