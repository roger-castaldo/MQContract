using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.RabbitMQ;
using RabbitMQ.Client;

namespace ConnectorTesting;

[TestClass]
public class RabbitMQConnectorTests
{
    private static readonly string[] RequiredTopics = new[]
    {
        "Announcement",
        "Prompt",
        BulkPubSubTestHelper.BulkPubSubTopic
    };
    private static RabbitMQTestHarness? rabbitMQTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        rabbitMQTestHarness = new RabbitMQTestHarness();
        await rabbitMQTestHarness.StartAsync();
        await using var serviceConnection = GetConnection();
        foreach (var topic in RequiredTopics)
            await serviceConnection.ExchangeDeclareAsync(topic, ExchangeType.Fanout);
    }

    private static Connection GetConnection()
    {
        Assert.IsNotNull(rabbitMQTestHarness);
        var factory = new ConnectionFactory()
        {
            ClientProvidedName = "TestConnection",
            Uri = new Uri(rabbitMQTestHarness.ConnectionString)
        };
        return new(factory);
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (rabbitMQTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

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
