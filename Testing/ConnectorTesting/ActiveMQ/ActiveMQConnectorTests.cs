using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.ActiveMQ;

namespace ConnectorTesting;

[TestClass]
public class ActiveMQConnectorTests
{
    private static ActiveMQTestHarness? activeMQTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        activeMQTestHarness = new ActiveMQTestHarness();
        await activeMQTestHarness.StartAsync();
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (activeMQTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    [TestMethod]
    public async Task TestPubSub()
    {
        Assert.IsNotNull(activeMQTestHarness);
        var connection = new Connection(new Uri(activeMQTestHarness.BrokerAddress), activeMQTestHarness.UserName, activeMQTestHarness.Password);
        await PubSubTestHelper.ExecutePubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestBulkPubSub()
    {
        Assert.IsNotNull(activeMQTestHarness);
        var connection = new Connection(new Uri(activeMQTestHarness.BrokerAddress), activeMQTestHarness.UserName, activeMQTestHarness.Password);
        await BulkPubSubTestHelper.ExecuteBulkPubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestQueryResponse()
    {
        Assert.IsNotNull(activeMQTestHarness);
        var connection = new Connection(new Uri(activeMQTestHarness.BrokerAddress), activeMQTestHarness.UserName, activeMQTestHarness.Password);
        await QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestPing()
    {
        Assert.IsNotNull(activeMQTestHarness);
        var connection = new Connection(new Uri(activeMQTestHarness.BrokerAddress), activeMQTestHarness.UserName, activeMQTestHarness.Password);
        await PingTestHelper.ExecutePingTestsAsync(connection);
    }
}
