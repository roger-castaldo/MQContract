using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.Redis;
using StackExchange.Redis;

namespace ConnectorTesting;

[TestClass]
public class RedisConnectorTests
{
    private static RedisTestHarness? redisTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        redisTestHarness = new RedisTestHarness();
        await redisTestHarness.StartAsync();
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (redisTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    [TestMethod]
    public async Task TestPubSub()
    {
        Assert.IsNotNull(redisTestHarness);
        var conf = new ConfigurationOptions();
        conf.EndPoints.Add(redisTestHarness.Endpoints);
        var connection = new Connection(conf);
        await PubSubTestHelper.ExecutePubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestQueryResponse()
    {
        Assert.IsNotNull(redisTestHarness);
        var conf = new ConfigurationOptions();
        conf.EndPoints.Add(redisTestHarness.Endpoints);
        var connection = new Connection(conf);
        await QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestPing()
    {
        Assert.IsNotNull(redisTestHarness);
        var conf = new ConfigurationOptions();
        conf.EndPoints.Add(redisTestHarness.Endpoints);
        var connection = new Connection(conf);
        await PingTestHelper.ExecutePingTestsAsync(connection);
    }
}
