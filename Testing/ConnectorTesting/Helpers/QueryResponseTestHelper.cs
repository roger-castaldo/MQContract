using ConnectorTesting.Messages;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace ConnectorTesting.Helpers;

internal static class QueryResponseTestHelper
{
    private static readonly IEnumerable<(Prompt message, MessageHeader? header)> TestPrompts = [
        new (new Prompt("Bob","Loblaw"), new MessageHeader([new("test-key1", "test-value1")])),
        new (new Prompt("Fred","Flintstone"), new MessageHeader([new("test-key2", "test-value2")])),
        new (new Prompt("Barney","Rubble"), new MessageHeader([new("test-key3", "test-value3")]))
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
            return new(new($"Greeting {message.Message.FirstName} {message.Message.LastName}"), message.Headers.AsEnumerable());
        }, error =>
        {
            errors.Add(error);
        },
        group: "TestGroup");

        Assert.IsNotNull(subscription);

        await Task.Delay(TimeSpan.FromSeconds(30));

        var results = new List<QueryResult<Reply>>();

        foreach (var (message, header) in TestPrompts)
            results.Add(await contractConnection.QueryAsync<Prompt, Reply>(message, timeout: TimeSpan.FromMinutes(1), messageHeader: header));

        Assert.IsTrue(results.All(r => !r.IsError));

        Assert.AreEqual(TestPrompts.Count(), receivedMessages.Count);
        Assert.AreEqual(0, errors.Count);
        foreach (var (prompt, header) in TestPrompts)
        {
            var receivedMessage = receivedMessages.FirstOrDefault(m => Equals(m.Message, prompt));
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(prompt, receivedMessage.Message);
            Assert.AreEqual(header!.Count, receivedMessage.Headers.Count);
            Assert.IsTrue(header.AsEnumerable().All(h => Equals(h.Value, receivedMessage.Headers[h.Key])));
        }

        for (var x = 0; x<results.Count; x++)
        {
            var result = results[x];
            var testPrompt = TestPrompts.ElementAt(x);
            Assert.AreEqual($"Greeting {testPrompt.message.FirstName} {testPrompt.message.LastName}", result.Result?.Greeting);
            Assert.AreEqual(testPrompt.header!.Count, result.Header.Count);
            Assert.IsTrue(testPrompt.header.AsEnumerable().All(h => Equals(h.Value, result.Header[h.Key])));
        }
    }
}
