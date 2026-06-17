using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.AzureServiceBus;

namespace ConnectorTesting;

[TestClass]
public class AzureServiceBusConnectorTests
{
    private static AzureServiceBusTestHarness? azureServiceBusTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        azureServiceBusTestHarness = new AzureServiceBusTestHarness();
        await azureServiceBusTestHarness.StartAsync();
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (azureServiceBusTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    [TestMethod]
    public async Task TestPubSub()
    {
        Assert.IsNotNull(azureServiceBusTestHarness);
        var connection = new Connection(new(azureServiceBusTestHarness.ConnectionString));
        await PubSubTestHelper.ExecutePubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestBulkPubSub()
    {
        Assert.IsNotNull(azureServiceBusTestHarness);
        var connection = new Connection(new(azureServiceBusTestHarness.ConnectionString));
        await BulkPubSubTestHelper.ExecuteBulkPubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestQueryResponse()
    {
        Assert.IsNotNull(azureServiceBusTestHarness);
        var connection = new Connection(new(azureServiceBusTestHarness.ConnectionString));
        await QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestPing()
    {
        Assert.IsNotNull(azureServiceBusTestHarness);
        var connection = new Connection(new(azureServiceBusTestHarness.ConnectionString));
        await PingTestHelper.ExecutePingTestsAsync(connection);
    }
}
