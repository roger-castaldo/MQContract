using ConnectorTesting.Harnesses;
using ConnectorTesting.Helpers;
using MQContract.InMemory;

namespace ConnectorTesting;

[TestClass]
public class InMemoryConnectorTests
{
    [TestMethod]
    public async Task TestPubSub()
    {
        var connection = new Connection();
        await PubSubTestHelper.ExecutePubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestBulkPubSub()
    {
        var connection = new Connection();
        await BulkPubSubTestHelper.ExecuteBulkPubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestQueryResponse()
    {
        var connection = new Connection();
        await QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestPing()
    {
        var connection = new Connection();
        await PingTestHelper.ExecutePingTestsAsync(connection);
    }
}
