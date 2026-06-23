using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.AmazonSNQS;

namespace ConnectorTesting;

[TestClass]
public class AmazonSQNSConnectorTests
{
    private static readonly string[] RequiredTopics = new[]
    {
        "Announcement",
        "Prompt",
        "PromptResponse"
    };
    private const string QueueAttributeName = "QueueArn";
    private static AmazonSQNSTestHarness? amazonSQNSTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        amazonSQNSTestHarness = new AmazonSQNSTestHarness();
        await amazonSQNSTestHarness.StartAsync();
        await using var serviceConnection = GetConnection();
        foreach (var topic in RequiredTopics)
        {
            var snsResponse = await serviceConnection.SNSClient!.CreateTopicAsync(topic);
            var sqsResponse = await serviceConnection.SQSClient!.CreateQueueAsync(topic);
            var queueArn = (await serviceConnection.SQSClient!.GetQueueAttributesAsync(sqsResponse.QueueUrl, [QueueAttributeName])).QueueARN;
            await serviceConnection.SQSClient!.SetQueueAttributesAsync(sqsResponse.QueueUrl, new() {
    { "Policy",$@"{{
            ""Version"": ""2012-10-17"",
            ""Statement"": [
                {{
                    ""Effect"": ""Allow"",
                    ""Principal"": {{ ""Service"": ""sns.amazonaws.com"" }},
                    ""Action"": ""sqs:SendMessage"",
                    ""Resource"": ""{queueArn}"",
                    ""Condition"": {{
                        ""ArnEquals"": {{ ""aws:SourceArn"": ""{snsResponse.TopicArn}"" }}
                    }}
                }}
            ]
        }}"}
});
            await serviceConnection.SNSClient.SubscribeAsync(snsResponse.TopicArn, "sqs", queueArn);
        }
    }

    [ClassCleanup]
    public static async Task Cleanup()
        => await (amazonSQNSTestHarness?.DisposeAsync()??ValueTask.CompletedTask);

    private static Connection GetConnection()
    {
        Assert.IsNotNull(amazonSQNSTestHarness);
        var config = new AmazonSimpleNotificationServiceConfig
        {
            ServiceURL = amazonSQNSTestHarness.ConnectionString,
            AuthenticationRegion = "us-east-1",
            DefaultAWSCredentials = new BasicAWSCredentials("test","test")
        };
        var sqsConfig = new AmazonSQSConfig
        {
            ServiceURL = amazonSQNSTestHarness.ConnectionString,
            AuthenticationRegion = "us-east-1",
            DefaultAWSCredentials = new BasicAWSCredentials("test", "test")
        };
        return new(snsClientConfiguration: config, sqsClientConfiguration: sqsConfig);
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
