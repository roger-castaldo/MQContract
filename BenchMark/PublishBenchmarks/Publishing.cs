using BenchMark.Messages;
using BenchmarkDotNet.Attributes;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Text;

namespace BenchMark.PublishBenchmarks
{
    [MemoryDiagnoser]
    public class Publishing
    {
        public const string ChannelName = "sample";
        private const string MessageContent = "The quick brown fox";
        private IMessageServiceConnection? serviceConnection;
        private IContractConnection? contractConnection;
        private Announcement? announcement;
        private EncodedAnnouncement? encodedAnnouncement;
        private MyMessageContext? myMessageContext;

        [GlobalSetup]
        public void Setup()
        {
            serviceConnection = new FakePublishConnection();
            contractConnection = ContractConnection.Instance(serviceConnection);
            announcement = new(MessageContent);
            encodedAnnouncement = new(MessageContent);
            myMessageContext = new();
        }

        private static async Task ExecuteOperationsAsync(Func<Task> operation)
        {
            for (var x = 0; x<Constants.PublishCount; x++)
                await operation();
        }

        [Benchmark(Baseline = true)]
        public async Task PublishDirectlyToConnection()
            => await ExecuteOperationsAsync(async () =>
            {
                _ = await serviceConnection!.PublishAsync(new(Guid.NewGuid().ToString(), "sample", ChannelName, new([]), ASCIIEncoding.ASCII.GetBytes(MessageContent)));
            });

        [Benchmark]
        public async Task PublishBasicEncodedMessage()
            => await ExecuteOperationsAsync(async () =>
            {
                _ = await contractConnection!.PublishAsync<string>(new TransmissionMessage<string>(MessageContent), channel: ChannelName);
            });

        [Benchmark]
        public async Task PublishDefaultEncodedMessage()
            => await ExecuteOperationsAsync(async () =>
            {
                _ = await contractConnection!.PublishAsync<Announcement>(new TransmissionMessage<Announcement>(announcement!), channel: ChannelName);
            });

        [Benchmark]
        public async Task PublishCustomEncodedMessage()
            => await ExecuteOperationsAsync(async () =>
            {
                _ = await contractConnection!.PublishAsync<EncodedAnnouncement>(new TransmissionMessage<EncodedAnnouncement>(encodedAnnouncement!), channel: ChannelName);
            });

        [Benchmark]
        public async Task PublishBasicEncodedMessageWithContext()
        {
            await ((IContractedConnection)contractConnection!).RegisterMessageContextAsync(myMessageContext!);
            await ExecuteOperationsAsync(async () =>
            {
                _ = await contractConnection!.PublishAsync<string>(new TransmissionMessage<string>(MessageContent), channel: ChannelName);
            });
        }

        [Benchmark]
        public async Task PublishDefaultEncodedMessageWithContext()
        {
            await ((IContractedConnection)contractConnection!).RegisterMessageContextAsync(myMessageContext!);
            await ExecuteOperationsAsync(async () =>
            {
                _ = await contractConnection!.PublishAsync<Announcement>(new TransmissionMessage<Announcement>(announcement!), channel: ChannelName);
            });
        }

        [Benchmark]
        public async Task PublishCustomEncodedMessageWithContext()
        {
            await ((IContractedConnection)contractConnection!).RegisterMessageContextAsync(myMessageContext!);
            await ExecuteOperationsAsync(async () =>
            {
                _ = await contractConnection!.PublishAsync<EncodedAnnouncement>(new TransmissionMessage<EncodedAnnouncement>(encodedAnnouncement!), channel: ChannelName);
            });
        }
    }
}
