using BenchMark.Messages;
using BenchmarkDotNet.Attributes;
using MQContract;
using MQContract.Interfaces;
using MQContract.Messages;

namespace BenchMark.InMemoryBenchmarks
{
    [MemoryDiagnoser]
    public class SubscribingInMemory
    {
        private const string channel = "Announcements";
        private static readonly Announcement testMessage = new("The quick brown fox");

        [Params(1, 10, 100, 1000, 5000, 10000, 50000)]
        public int MessageCount { get; set; }
        private IContractedConnection? contractConnection;
        private TaskCompletionSource? completionSource;
        private ISubscription? subscription;

        [IterationSetup]
        public void SetupIteration()
        {
            var count = MessageCount;
            contractConnection = ContractConnection.Instance(new MQContract.InMemory.Connection());
            completionSource = new();
            var subTask = contractConnection.SubscribeAsync<Announcement>(
                (message) =>
                {
                    count--;
                    if (count<=0)
                        completionSource.TrySetResult();
                    return ValueTask.CompletedTask;
                },
                (err) => { },
                channel: channel
            ).AsTask();
            subTask.Wait();
            subscription = subTask.Result;
        }

        [IterationCleanup()]
        public void CleanupIteration()
        {
            subscription?.EndAsync().AsTask().Wait();
            contractConnection?.CloseAsync().AsTask().Wait();
        }

        [Benchmark]
        public async Task ReadAllSubscriptionMessages()
        {
            var count = MessageCount;
            for (var x = 0; x<count; x++)
                await contractConnection!.PublishAsync<Announcement>(new TransmissionMessage<Announcement>(testMessage), channel: channel);
            await completionSource!.Task;
        }
    }
}
