using DotNet.Testcontainers.Containers;
using Testcontainers.PubSub;

namespace ConnectorTesting.Harnesses;

internal class GooglePubSubTestHarness : AServiceHarness
{
    public string EndPoint { get; private set; } = string.Empty;

    protected override IContainer Build()
        => new PubSubBuilder("gcr.io/google.com/cloudsdktool/google-cloud-cli:446.0.1-emulators")
            .Build();

    protected override void PostStart(IContainer container)
    {
        EndPoint = ((PubSubContainer)container).GetEmulatorEndpoint();
    }
}
