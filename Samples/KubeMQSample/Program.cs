using Messages;
using MQContract;
using MQContract.KubeMQ;
using MQContract.Messages;

using var sourceCancel = new CancellationTokenSource();

Console.CancelKeyPress += delegate {
    sourceCancel.Cancel();
};

using var serviceConnection = new Connection(new ConnectionOptions()
{
    Logger=new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider().CreateLogger("Messages"),
    ClientId="KubeMQSample"
});

var contractConnection = new ContractConnection(serviceConnection);

using var arrivalSubscription = await contractConnection.SubscribeAsync<ArrivalAnnouncement>(
    (announcement) =>
    {
        Console.WriteLine($"Announcing the arrival of {announcement.Message.LastName}, {announcement.Message.FirstName}. [{announcement.ID},{announcement.RecievedTimestamp}]");
        return Task.CompletedTask;
    },
    (error) => Console.WriteLine($"Announcement error: {error.Message}"),
    cancellationToken: sourceCancel.Token
);

using var greetingSubscription = await contractConnection.SubscribeQueryResponseAsync<Greeting, string>(
    (greeting) =>
    {
        Console.WriteLine($"Greeting recieved for {greeting.Message.LastName}, {greeting.Message.FirstName}. [{greeting.ID},{greeting.RecievedTimestamp}]");
        System.Diagnostics.Debug.WriteLine($"Time to convert message: {greeting.ProcessedTimestamp.Subtract(greeting.RecievedTimestamp).TotalMilliseconds}ms");
        return Task.FromResult<QueryResponseMessage<string>>(
            new($"Welcome {greeting.Message.FirstName} {greeting.Message.LastName} to the KubeMQ sample")
        );
    },
    (error) => Console.WriteLine($"Greeting error: {error.Message}"),
    cancellationToken: sourceCancel.Token
);

var result = await contractConnection.PublishAsync<ArrivalAnnouncement>(new("Bob", "Loblaw"), cancellationToken:sourceCancel.Token);
Console.WriteLine($"Result 1 [Success:{!result.IsError}, ID:{result.MessageID}]");
result = await contractConnection.PublishAsync<ArrivalAnnouncement>(new("Fred", "Flintstone"), cancellationToken: sourceCancel.Token);
Console.WriteLine($"Result 2 [Success:{!result.IsError}, ID:{result.MessageID}]");

var response = await contractConnection.QueryAsync<Greeting,string>(new Greeting("Bob","Loblaw"),cancellationToken:sourceCancel.Token);
Console.WriteLine($"Response 1 [Success:{!response.IsError}, ID:{response.MessageID}, Response: {response.Result}]");
response = await contractConnection.QueryAsync<Greeting, string>(new Greeting("Fred", "Flintstone"), cancellationToken: sourceCancel.Token);
Console.WriteLine($"Response 2 [Success:{!response.IsError}, ID:{response.MessageID}, Response: {response.Result}]");

Console.WriteLine("Press Ctrl+C to close");

sourceCancel.Token.WaitHandle.WaitOne();
Console.WriteLine("System completed operation");