using ConnectorTesting.Messages;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace ConnectorTesting.Helpers;

internal static class QueryResponseTestHelper
{
    private static readonly IEnumerable<TransmissionMessage<Prompt>> TestPrompts = [
        new (new Prompt("Bob","Loblaw"), Header: new MessageHeader([new("test-key1", "test-value1")])),
        new (new Prompt("Fred","Flintstone"), Header: new MessageHeader([new("test-key2", "test-value2")])),
        new (new Prompt("Barney","Rubble"), Header: new MessageHeader([new("test-key3", "test-value3")]))
    ];

    public static async Task ExecuteQueryResponseTestsAsync(IMessageServiceConnection messageServiceConnection)
    {
        var receivedMessages = new List<IReceivedMessage<Prompt>>();
        var errors = new List<Exception>();
        await using var contractConnection = await ContractConnection.Instance(messageServiceConnection)
            .RegisterMessageContextAsync(new TestMessageContext());

        Assert.IsNotNull(contractConnection);

        await using var subscription = await contractConnection.SubscribeQueryAsyncResponseAsync<Prompt, Reply>(async message =>
        {
            receivedMessages.Add(message);
            return new(new($"Greeting {message.Message.FirstName} {message.Message.LastName}"), message.Headers.AsEnumerable().Select(pair=>new KeyValuePair<string, string?>(pair.Key,pair.Value)));
        }, error =>
        {
            errors.Add(error);
        },
        group: "TestGroup");

        Assert.IsNotNull(subscription);

        await Task.Delay(TimeSpan.FromSeconds(30));

        var results = new List<QueryResult<Reply>>();

        foreach (var message in TestPrompts)
            results.Add(await contractConnection.QueryAsync<Prompt, Reply>(message, timeout: TimeSpan.FromMinutes(1)));

        Assert.IsTrue(results.All(r => !r.IsError));

        Assert.HasCount(TestPrompts.Count(), receivedMessages);
        Assert.HasCount(TestPrompts.Count(), results);
        Assert.IsEmpty(errors);
        foreach (var message in TestPrompts)
        {
            var receivedMessage = receivedMessages.FirstOrDefault(m => Equals(m.Message, message.Message));
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(message.Message, receivedMessage.Message);
            Assert.AreEqual(message.Header!.Count, receivedMessage.Headers.Count);
            Assert.IsTrue(message.Header.AsEnumerable().All(h => Equals(h.Value, receivedMessage.Headers[h.Key])));
        }

        for (var x = 0; x<results.Count; x++)
        {
            var result = results[x];
            var message = TestPrompts.ElementAt(x);
            Assert.AreEqual($"Greeting {message.Message.FirstName} {message.Message.LastName}", result.Result?.Greeting);
            Assert.AreEqual(message.Header!.Count, result.Header.Count);
            Assert.IsTrue(message.Header.AsEnumerable().All(h => Equals(h.Value, result.Header[h.Key])));
        }
    }
}
