using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using MQContract.GooglePubSub;

namespace ConnectorTesting;

[TestClass]
public class GooglePubSubConnectorTests
{
    private static readonly string[] RequiredTopics = new[]
    {
        "Announcement",
        "Prompt",
        "PromptResponse",
        BulkPubSubTestHelper.BulkPubSubTopic
    };
    private const string projectId = "sample-project-id";
    private static GooglePubSubTestHarness? googlePubSubTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        googlePubSubTestHarness = new GooglePubSubTestHarness();
        await googlePubSubTestHarness.StartAsync();
        await using var serviceConnection = GetConnection();
        foreach(var topic in RequiredTopics)
        {
            var topicName = new TopicName(projectId, topic);
            try
            {
                if (await serviceConnection.PublisherServiceApi.GetTopicAsync(topicName)==null)
                    await serviceConnection.PublisherServiceApi.CreateTopicAsync(topicName);
            }catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                await serviceConnection.PublisherServiceApi.CreateTopicAsync(topicName);
            }
        }
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (googlePubSubTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    private static Connection GetConnection()
    {
        Assert.IsNotNull(googlePubSubTestHarness);
        var publisherServiceBuilder = new PublisherServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            Endpoint=googlePubSubTestHarness.EndPoint,
            ChannelCredentials = ChannelCredentials.Insecure
        };

        var subscriberServiceBuilder = new SubscriberServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            Endpoint=googlePubSubTestHarness.EndPoint,
            ChannelCredentials = ChannelCredentials.Insecure
        };

        return new Connection(projectId, publisherServiceBuilder, subscriberServiceBuilder);
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
