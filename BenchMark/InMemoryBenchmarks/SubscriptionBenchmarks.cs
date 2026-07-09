using BenchMark.Messages;
using BenchmarkDotNet.Attributes;
using MQContract;
using MQContract.CQRS.Extensions;
using MQContract.Messages;

namespace BenchMark.InMemoryBenchmarks;

[MemoryDiagnoser]
public class SubscriptionBenchmarks
{
    private const string channel = "Announcements";
    private static readonly Announcement testMessage = new("The quick brown fox");
    private static readonly AnnouncementCommand testCommand = new("The quick brown fox");

    [Benchmark(Baseline = true)]
    public async Task RunAsSubscription()
    {
        var count = Constants.PublishCount;
        await using var contractConnection = ContractConnection.Instance(new MQContract.InMemory.Connection());
        var completionSource = new TaskCompletionSource();

        using var subscription = await contractConnection.SubscribeAsync<Announcement>(
            (message) =>
            {
                count--;
                if (count<=0)
                    completionSource.TrySetResult();
                return ValueTask.CompletedTask;
            },
            (err) => { },
            channel: channel
        );

        await Task.WhenAll(Enumerable.Range(0, Constants.PublishCount)
            .Select(c => contractConnection.PublishAsync<Announcement>(new TransmissionMessage<Announcement>(testMessage), channel: channel).AsTask())
        );

        await completionSource.Task;

        await subscription.EndAsync();
        await contractConnection.CloseAsync();
    }

    [Benchmark()]
    public async Task RunAsConsumer()
    {
        var count = Constants.PublishCount;
        var completionSource = new TaskCompletionSource();
        await using var contractConnection = ContractConnection.Instance(new MQContract.InMemory.Connection());
        await contractConnection.RegisterPubSubAsyncConsumerAsync<Announcement, AnnouncementConsumer>(new AnnouncementConsumer(count, completionSource), channel: channel);

        await Task.WhenAll(Enumerable.Range(0, Constants.PublishCount)
            .Select(c => contractConnection.PublishAsync<Announcement>(new TransmissionMessage<Announcement>(testMessage), channel: channel).AsTask())
        );

        await completionSource.Task;

        await contractConnection.CloseAsync();
    }

    [Benchmark()]
    public async Task RunAsCommand()
    {
        var count = Constants.PublishCount;
        var completionSource = new TaskCompletionSource();
        await using var contractConnection = ContractConnection.Instance(new MQContract.InMemory.Connection());
        var cqrsConnection = contractConnection.CreateCQRSConnection();
        await cqrsConnection.RegisterCommandProcessorAsync<AnnouncementCommand>(new AnnouncementCommandProcessor(count, completionSource));

        await Task.WhenAll(Enumerable.Range(0, Constants.PublishCount)
            .Select(c => cqrsConnection.ExecuteCommandAsync(testCommand).AsTask())
        );

        await completionSource.Task;

        await contractConnection.CloseAsync();
    }

    [Benchmark()]
    public async Task RunAsSubscriptionWithContext()
    {
        var count = Constants.PublishCount;
        await using var contractConnection = ContractConnection.Instance(new MQContract.InMemory.Connection());
        await contractConnection.RegisterMessageContextAsync(new MyMessageContext());
        var completionSource = new TaskCompletionSource();

        using var subscription = await contractConnection.SubscribeAsync<Announcement>(
            (message) =>
            {
                count--;
                if (count<=0)
                    completionSource.TrySetResult();
                return ValueTask.CompletedTask;
            },
            (err) => { },
            channel: channel
        );

        await Task.WhenAll(Enumerable.Range(0, Constants.PublishCount)
            .Select(c => contractConnection.PublishAsync<Announcement>(new TransmissionMessage<Announcement>(testMessage), channel: channel).AsTask())
        );

        await completionSource.Task;

        await subscription.EndAsync();
        await contractConnection.CloseAsync();
    }

    [Benchmark()]
    public async Task RunAsConsumerWithContext()
    {
        var count = Constants.PublishCount;
        var completionSource = new TaskCompletionSource();
        await using var contractConnection = ContractConnection.Instance(new MQContract.InMemory.Connection());
        await contractConnection.RegisterMessageContextAsync(new MyMessageContext());
        await contractConnection.RegisterPubSubAsyncConsumerAsync<Announcement, AnnouncementConsumer>(new AnnouncementConsumer(count, completionSource), channel: channel);

        await Task.WhenAll(Enumerable.Range(0, Constants.PublishCount)
            .Select(c => contractConnection.PublishAsync<Announcement>(new TransmissionMessage<Announcement>(testMessage), channel: channel).AsTask())
        );

        await completionSource.Task;

        await contractConnection.CloseAsync();
    }

    [Benchmark()]
    public async Task RunAsCommandWithContext()
    {
        var count = Constants.PublishCount;
        var completionSource = new TaskCompletionSource();
        await using var contractConnection = ContractConnection.Instance(new MQContract.InMemory.Connection());
        await contractConnection.RegisterMessageContextAsync(new MyMessageContext());
        var cqrsConnection = contractConnection.CreateCQRSConnection();
        await cqrsConnection.RegisterCommandProcessorAsync<AnnouncementCommand>(new AnnouncementCommandProcessor(count, completionSource));

        await Task.WhenAll(Enumerable.Range(0, Constants.PublishCount)
            .Select(c => cqrsConnection.ExecuteCommandAsync(testCommand).AsTask())
        );

        await completionSource.Task;

        await contractConnection.CloseAsync();
    }
}
