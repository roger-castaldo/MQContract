using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.AzureServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace ConnectorTesting;

[TestClass]
public class AzureServiceBusConnectorTests
{
    private static readonly string[][] RequiredTopics = 
    [
        ["Announcement", "TestGroup"],
        ["Prompt", "TestQueryGroup"],
        ["PromptResponse", "TestGroup"],
        [BulkPubSubTestHelper.BulkPubSubTopic, "TestBulkGroup"]
    ];
    private static AzureServiceBusTestHarness? azureServiceBusTestHarness;

    [ClassInitialize]
    public static async Task Init(TestContext testContext)
    {
        azureServiceBusTestHarness = new AzureServiceBusTestHarness();
        await azureServiceBusTestHarness.StartAsync();
        // Ensure topics exist in the Service Bus namespace used by the test harness.
        
        var adminClient = new ServiceBusAdministrationClient(azureServiceBusTestHarness.AdminConnectionString);
        foreach (var pair in RequiredTopics)
        {
            try
            {
                using var ctsCreate = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await adminClient.CreateTopicAsync(pair[0], ctsCreate.Token);
                testContext.WriteLine($"Created topic '{pair[0]}'");
                await adminClient.CreateSubscriptionAsync(new(pair[0], pair[1]));
            }
            catch (Azure.Messaging.ServiceBus.ServiceBusException sbEx) when (
                sbEx.Reason == Azure.Messaging.ServiceBus.ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                // Topic already exists; nothing to do.
            }
            catch (Exception ex)
            {
                testContext.WriteLine($"Warning: failed to ensure topic '{pair[0]}': {ex.Message}");
            }
        }
        await adminClient.CreateTopicAsync("QueryResponse.Inbox");
        await adminClient.CreateSubscriptionAsync(new("QueryResponse.Inbox", "QueryResponse.Inbox")
        {
            RequiresSession=true
        });
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
