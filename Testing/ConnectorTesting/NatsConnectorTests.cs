using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.NATS;

namespace ConnectorTesting;

[TestClass]
public class NatsConnectorTests
{
    private static NatsTestHarness? natsTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        natsTestHarness = new NatsTestHarness();
        await natsTestHarness.StartAsync();
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (natsTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    [TestMethod]
    public async Task TestPubSub()
    {
        Assert.IsNotNull(natsTestHarness);
        var connection = new Connection(natsTestHarness.Options);
        await PubSubTestHelper.ExecutePubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestQueryResponse()
    {
        Assert.IsNotNull(natsTestHarness);
        var connection = new Connection(natsTestHarness.Options);
        await QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestPing()
    {
        Assert.IsNotNull(natsTestHarness);
        var connection = new Connection(natsTestHarness.Options);
        await PingTestHelper.ExecutePingTestsAsync(connection);
    }
}
