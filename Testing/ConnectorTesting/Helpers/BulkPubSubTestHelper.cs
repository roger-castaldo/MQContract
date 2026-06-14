using ConnectorTesting.Messages;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace ConnectorTesting.Helpers;

internal static class BulkPubSubTestHelper
{
    private static readonly IEnumerable<(Announcement message, MessageHeader? header)> TestAnnouncements = [
        new (new Announcement("Hello World!"), new MessageHeader([new("test-key1", "test-value1")])),
        new (new Announcement("Welcome to Pub/Sub testing."), new MessageHeader([new("test-key2", "test-value2")])),
        new (new Announcement("This is a test message."), new MessageHeader([new("test-key3", "test-value3")])),
        new (new Announcement("MQContract is great for messaging!"), new MessageHeader([new("test-key4", "test-value4")])),
        new (new Announcement("Testing message delivery and error handling."), new MessageHeader([new("test-key5", "test-value5")]))
    ];

    public static async Task ExecuteBulkPubSubTestsAsync(IMessageServiceConnection messageServiceConnection)
    {
        var receivedMessages = new List<IReceivedMessage<Announcement>>();
        var errors = new List<Exception>();
        await using var contractConnection = await ContractConnection.Instance(messageServiceConnection)
            .RegisterMessageContextAsync(new TestMessageContext());

        Assert.IsNotNull(contractConnection);

        await using var subscription = await contractConnection.SubscribeAsync<Announcement>(async message =>
        {
            receivedMessages.Add(message);
            await Task.CompletedTask;
        }, error =>
        {
            errors.Add(error);
        });

        Assert.IsNotNull(subscription);

        await Task.Delay(TimeSpan.FromSeconds(30));

        var results = await contractConnection.BulkPublishAsync(TestAnnouncements);

        Assert.IsTrue(results.All(r => !r.IsError));

        var success = await TestHelper.WaitForCount(receivedMessages, TestAnnouncements.Count(), TimeSpan.FromMinutes(2));

        Assert.IsTrue(success);

        Assert.AreEqual(TestAnnouncements.Count(), receivedMessages.Count);
        Assert.AreEqual(0, errors.Count);
        foreach(var (announcement, header) in TestAnnouncements)
        {
            var receivedMessage = receivedMessages.FirstOrDefault(m => Equals(m.Message.Message, announcement.Message));
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(announcement.Message, receivedMessage.Message.Message);
            Assert.AreEqual(header!.Count, receivedMessage.Headers.Count);
            Assert.IsTrue(header.AsEnumerable().All(h => Equals(h.Value, receivedMessage.Headers[h.Key])));
        }

        await subscription.EndAsync();
        await contractConnection.CloseAsync();
    }

}
