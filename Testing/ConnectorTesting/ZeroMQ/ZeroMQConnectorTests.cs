
using ConnectorTesting.Helpers;
using MQContract.ZeroMQ;
using System.Security.Cryptography;

namespace ConnectorTesting;

[TestClass]
public class ZeroMQConnectorTests
{
    private int BindServer(Connection connection)
    {
        var pubPort = RandomNumberGenerator.GetInt32(10000, 11000);
        while (true)
        {
            try
            {
                connection.BindAsServer($"tcp://localhost:{pubPort}");
                break;
            }
            catch (Exception) {
                pubPort = RandomNumberGenerator.GetInt32(10000, 11000);
            }
        }
        return pubPort;
    }

    private static void BindInbox(Connection connection)
    {
        var inboxPort = RandomNumberGenerator.GetInt32(11001, 12000);
        while (true)
        {
            try
            {
                connection.BindInboxAddress($"tcp://localhost:{inboxPort}");
                break;
            }
            catch (Exception)
            {
                inboxPort = RandomNumberGenerator.GetInt32(10000, 11000);
            }
        }
    }

    [TestMethod]
    public async Task TestPubSub()
    {
        await using var connection = new Connection();
        var pubPort = BindServer(connection);
        connection.ConnectToServer($"tcp://localhost:{pubPort}");
        await PubSubTestHelper.ExecutePubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestBulkPubSub()
    {
        await using var connection = new Connection();
        var pubPort = BindServer(connection);
        connection.ConnectToServer($"tcp://localhost:{pubPort}");
        await BulkPubSubTestHelper.ExecuteBulkPubSubTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestQueryResponse()
    {
        await using var connection = new Connection();
        var pubPort = BindServer(connection);
        BindInbox(connection);
        connection.ConnectToServer($"tcp://localhost:{pubPort}");
        await QueryResponseTestHelper.ExecuteQueryResponseTestsAsync(connection);
    }

    [TestMethod]
    public async Task TestPing()
    {
        await using var connection = new Connection();
        var pubPort = BindServer(connection);
        BindInbox(connection);
        connection.ConnectToServer($"tcp://localhost:{pubPort}");
        await PingTestHelper.ExecutePingTestsAsync(connection);
    }
}
