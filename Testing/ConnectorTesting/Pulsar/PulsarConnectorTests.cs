using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using DotPulsar;
using MQContract.ApachePulsar;

namespace ConnectorTesting;

[TestClass]
public class PulsarConnectorTests
{
    private static PulsarTestHarness? pulsarTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        pulsarTestHarness = new PulsarTestHarness();
        await pulsarTestHarness.StartAsync();
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (pulsarTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    [TestMethod]
    public async Task TestPubSub()
    {
        Assert.IsNotNull(pulsarTestHarness);
        var builder = PulsarClient.Builder()
            .ServiceUrl(new(pulsarTestHarness.BrokerAddress));
        var connection = new Connection(builder);
        await PubSubTestHelper.ExecutePubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestBulkPubSub()
    {
        Assert.IsNotNull(pulsarTestHarness);
        var builder = PulsarClient.Builder()
            .ServiceUrl(new(pulsarTestHarness.BrokerAddress));
        var connection = new Connection(builder);
        await BulkPubSubTestHelper.ExecuteBulkPubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestQueryResponse()
    {
        Assert.IsNotNull(pulsarTestHarness);
        var builder = PulsarClient.Builder()
            .ServiceUrl(new(pulsarTestHarness.BrokerAddress));
        var connection = new Connection(builder);
        await QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestPing()
    {
        Assert.IsNotNull(pulsarTestHarness);
        var builder = PulsarClient.Builder()
            .ServiceUrl(new(pulsarTestHarness.BrokerAddress));
        var connection = new Connection(builder);
        await PingTestHelper.ExecutePingTestsAsync(connection);
    }
}
