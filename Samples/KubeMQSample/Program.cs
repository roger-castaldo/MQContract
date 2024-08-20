﻿using Messages;
using MQContract;
using MQContract.KubeMQ;
using MQContract.KubeMQ.Options;
using MQContract.Messages;

using var sourceCancel = new CancellationTokenSource();

Console.CancelKeyPress += delegate {
    sourceCancel.Cancel();
};

await using var serviceConnection = new Connection(new ConnectionOptions()
{
    Logger=new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider().CreateLogger("Messages"),
    ClientId="KubeMQSample"
});

var contractConnection = new ContractConnection(serviceConnection);

await using var arrivalSubscription = await contractConnection.SubscribeAsync<ArrivalAnnouncement>(
    (announcement) =>
    {
        Console.WriteLine($"Announcing the arrival of {announcement.Message.LastName}, {announcement.Message.FirstName}. [{announcement.ID},{announcement.RecievedTimestamp}]");
        return Task.CompletedTask;
    },
    (error) => Console.WriteLine($"Announcement error: {error.Message}"),
    cancellationToken: sourceCancel.Token
);

await using var greetingSubscription = await contractConnection.SubscribeQueryResponseAsync<Greeting, string>(
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

await using var storedArrivalSubscription = await contractConnection.SubscribeAsync<StoredArrivalAnnouncement>(
    (announcement) =>
    {
        Console.WriteLine($"Stored Announcing the arrival of {announcement.Message.LastName}, {announcement.Message.FirstName}. [{announcement.ID},{announcement.RecievedTimestamp}]");
        return Task.CompletedTask;
    },
    (error) => Console.WriteLine($"Stored Announcement error: {error.Message}"),
    options:new StoredEventsSubscriptionOptions(StoredEventsSubscriptionOptions.MessageReadStyle.StartNewOnly),
    cancellationToken: sourceCancel.Token
);

var result = await contractConnection.PublishAsync<ArrivalAnnouncement>(new("Bob", "Loblaw"), cancellationToken:sourceCancel.Token);
Console.WriteLine($"Result 1 [Success:{!result.IsError}, ID:{result.ID}]");
result = await contractConnection.PublishAsync<ArrivalAnnouncement>(new("Fred", "Flintstone"), cancellationToken: sourceCancel.Token);
Console.WriteLine($"Result 2 [Success:{!result.IsError}, ID:{result.ID}]");

var response = await contractConnection.QueryAsync<Greeting,string>(new Greeting("Bob","Loblaw"),cancellationToken:sourceCancel.Token);
Console.WriteLine($"Response 1 [Success:{!response.IsError}, ID:{response.ID}, Response: {response.Result}]");
response = await contractConnection.QueryAsync<Greeting, string>(new Greeting("Fred", "Flintstone"), cancellationToken: sourceCancel.Token);
Console.WriteLine($"Response 2 [Success:{!response.IsError}, ID:{response.ID}, Response: {response.Result}]");

var storedResult = await contractConnection.PublishAsync<StoredArrivalAnnouncement>(new("Bob","Loblaw"),options:new PublishChannelOptions(true), cancellationToken:sourceCancel.Token);
Console.WriteLine($"Stored Result 1 [Success:{!storedResult.IsError}, ID:{storedResult.ID}]");
storedResult = await contractConnection.PublishAsync<StoredArrivalAnnouncement>(new("Fred", "Flintstone"), options: new PublishChannelOptions(true), cancellationToken: sourceCancel.Token);
Console.WriteLine($"Stored Result 2 [Success:{!storedResult.IsError}, ID:{storedResult.ID}]");

Console.WriteLine("Press Ctrl+C to close");

sourceCancel.Token.WaitHandle.WaitOne();
Console.WriteLine("System completed operation");