using BenchmarkDotNet.Attributes;
using MQContract;
using MQContract.Interfaces;

namespace BenchMark.PublishBenchmarks
{
    [MemoryDiagnoser]
    public class PublishingWithMiddleware
    {
        [Params("", "Metrics", "OTEL")]
        public string Middleware { get; set; } = string.Empty;

        public const string ChannelName = "sample";
        private const string MessageContent = "The quick brown fox";
        private IContractedConnection? contractConnection;

        [GlobalSetup]
        public void Setup()
        {
            contractConnection = ContractConnection.Instance(new FakePublishConnection());
            switch (Middleware)
            {
                case "Metrics":
                    contractConnection.AddMetrics(null, true);
                    break;
                case "OTEL":
                    contractConnection.EnableOpenTelemetry();
                    break;
                default:
                    break;
            }
        }

        [Benchmark]
        public async Task PublishBasicEncodedMessage()
        {
            _ = await contractConnection!.PublishAsync<string>(MessageContent, channel: ChannelName);
        }
    }
}