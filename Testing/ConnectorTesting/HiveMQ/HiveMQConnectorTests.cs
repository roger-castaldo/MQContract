using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.HiveMQ;

namespace ConnectorTesting;

[TestClass]
public class HiveMQConnectorTests
{
    private static HiveMQTestHarness? hiveMQTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        hiveMQTestHarness = new();
        await hiveMQTestHarness.StartAsync();
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (hiveMQTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    private static Connection GetConnection()
    {
        Assert.IsNotNull(hiveMQTestHarness);
        return new(new()
        {
            Host = hiveMQTestHarness.HostName,
            Port = hiveMQTestHarness.HostPort,
            CleanStart = false,
            ClientId = "TestClient"
        });
    }

    [TestMethod]
    public Task TestPubSub()
        => PubSubTestHelper.ExecutePubSubTestsAsync(GetConnection());

    [TestMethod]
    public Task TestBulkPubSub()
        => BulkPubSubTestHelper.ExecuteBulkPubSubTestsAsync(GetConnection());

    [TestMethod]
    public Task TestQueryResponse()
        => QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(GetConnection());

    [TestMethod]
    public Task TestPing()
        => PingTestHelper.ExecutePingTestsAsync(GetConnection());
}
