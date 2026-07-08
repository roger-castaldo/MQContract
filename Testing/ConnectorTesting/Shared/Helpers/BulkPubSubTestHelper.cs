using ConnectorTesting.Messages;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace ConnectorTesting.Helpers;

internal static class BulkPubSubTestHelper
{
    public static string BulkPubSubTopic = "BulkAnnouncement";

    private static readonly IEnumerable<TransmissionMessage<Announcement>> TestAnnouncements = [
        new (new Announcement("Hello World!"), Header: new MessageHeader([new("test-key1", "test-value1")])),
        new (new Announcement("Welcome to Pub/Sub testing."), Header: new MessageHeader([new("test-key2", "test-value2")])),
        new (new Announcement("This is a test message."), Header: new MessageHeader([new("test-key3", "test-value3")])),
        new (new Announcement("MQContract is great for messaging!"), Header: new MessageHeader([new("test-key4", "test-value4")])),
        new (new Announcement("Testing message delivery and error handling."), Header: new MessageHeader([new("test-key5", "test-value5")]))
    ];

    public static async Task ExecuteBulkPubSubTestsAsync(IMessageServiceConnection messageServiceConnection)
    {
        var receivedMessages = new List<IReceivedMessage<Announcement>>();
        var errors = new List<Exception>();
        var contractConnection = await TestHelper.CreateContractConnectionAsync(messageServiceConnection);

        var subscription = await contractConnection.SubscribeAsync<Announcement>(async message =>
        {
            receivedMessages.Add(message);
            await Task.CompletedTask;
        }, error =>
        {
            errors.Add(error);
        },
        group:"TestBulkGroup",
        channel: BulkPubSubTopic);

        Assert.IsNotNull(subscription);

        await Task.Delay(TimeSpan.FromSeconds(30));

        var results = await contractConnection.BulkPublishAsync(TestAnnouncements, channel: BulkPubSubTopic);

        Assert.IsTrue(results.All(r => !r.IsError), message: $"Publish failed {results.First(r => r.IsError).Error}");

        var success = await TestHelper.WaitForCount(receivedMessages, TestAnnouncements.Count(), TimeSpan.FromMinutes(2));

        await TestHelper.Cleanup(contractConnection, subscription);

        Assert.IsTrue(success);

        Assert.AreEqual(TestAnnouncements.Count(), receivedMessages.Count);
        Assert.AreEqual(0, errors.Count);
        foreach(var message in TestAnnouncements)
        {
            var receivedMessage = receivedMessages.FirstOrDefault(m => Equals(m.Message.Message, message.Message.Message));
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(message.Message.Message, receivedMessage.Message.Message);
            Assert.AreEqual(message.Header!.Count, receivedMessage.Headers.Count);
            Assert.IsTrue(message.Header.AsEnumerable().All(h => Equals(h.Value, receivedMessage.Headers[h.Key])));
        }
    }

}
