using BenchMark.Messages;
using BenchmarkDotNet.Attributes;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Service;
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

        [GlobalSetup]
        public void Setup()
        {
            serviceConnection = new FakePublishConnection();
            contractConnection = ContractConnection.Instance(serviceConnection);
            announcement = new(MessageContent);
            encodedAnnouncement = new(MessageContent);
        }

        [Benchmark(Baseline = true)]
        public async Task PublishDirectlyToConnection()
        {
            _ = await serviceConnection!.PublishAsync(new(Guid.NewGuid().ToString(), "sample", ChannelName, new([]), ASCIIEncoding.ASCII.GetBytes(MessageContent)));
        }

        [Benchmark]
        public async Task PublishBasicEncodedMessage()
        {
            _ = await contractConnection!.PublishAsync<string>(MessageContent, channel: ChannelName);
        }

        [Benchmark]
        public async Task PublishDefaultEncodedMessage()
        {
            _ = await contractConnection!.PublishAsync<Announcement>(announcement!, channel: ChannelName);
        }

        [Benchmark]
        public async Task PublishCustomEncodedMessage()
        {
            _ = await contractConnection!.PublishAsync<EncodedAnnouncement>(encodedAnnouncement!, channel: ChannelName);
        }
    }
}
