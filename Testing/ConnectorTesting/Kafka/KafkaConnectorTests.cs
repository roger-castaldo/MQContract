using Confluent.Kafka;
using Confluent.Kafka.Admin;
using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.Kafka;

namespace ConnectorTesting;

[TestClass]
public class KafkaConnectorTests
{
    private static readonly string[] RequiredTopics = new[]
    {
        "Announcement",
        "Prompt",
        "PromptResponse",
        BulkPubSubTestHelper.BulkPubSubTopic
    };
    private static KafkaTestHarness? kafkaTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        kafkaTestHarness = new KafkaTestHarness();
        await kafkaTestHarness.StartAsync();
        var config = new AdminClientConfig()
        {
            BootstrapServers = kafkaTestHarness.BootstrapAddress
        };
        using var adminClient = new AdminClientBuilder(config).Build();
        foreach(var topic in RequiredTopics)
        {
            var ts = new TopicSpecification
            {
                Name = topic,
                NumPartitions = 1,
                ReplicationFactor = 1
            };
            try
            {
                // Confluent's CreateTopicsAsync accepts TopicSpecification collection
                await adminClient.CreateTopicsAsync([ts], new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(30) });
            }
            catch (CreateTopicsException e)
            {
                // e.Results contains individual TopicResult with errors if creation fails for some topics
                foreach (var r in e.Results)
                {
                    if (r.Error.Code != ErrorCode.TopicAlreadyExists)
                    {
                        throw new Exception($"Failed to create topic {r.Topic}: {r.Error.Reason}");
                    }
                }
            }
        }
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (kafkaTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    [TestMethod]
    public async Task TestPubSub()
    {
        Assert.IsNotNull(kafkaTestHarness);
        var connection = new Connection(new()
        {
            ClientId="TestPubSub",
            BootstrapServers = kafkaTestHarness.BootstrapAddress
        });
        await PubSubTestHelper.ExecutePubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestBulkPubSub()
    {
        Assert.IsNotNull(kafkaTestHarness);
        var connection = new Connection(new()
        {
            ClientId="TestBulkPubSub",
            BootstrapServers = kafkaTestHarness.BootstrapAddress
        });
        await BulkPubSubTestHelper.ExecuteBulkPubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestQueryResponse()
    {
        Assert.IsNotNull(kafkaTestHarness);
        var connection = new Connection(new()
        {
            ClientId="TestQueryResponse",
            BootstrapServers = kafkaTestHarness.BootstrapAddress
        });
        await QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestPing()
    {
        Assert.IsNotNull(kafkaTestHarness);
        var connection = new Connection(new()
        {
            ClientId="TestPing",
            BootstrapServers = kafkaTestHarness.BootstrapAddress
        });
        await PingTestHelper.ExecutePingTestsAsync(connection);
    }
}
