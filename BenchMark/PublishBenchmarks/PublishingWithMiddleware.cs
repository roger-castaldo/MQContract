using BenchmarkDotNet.Attributes;
using MQContract;
using MQContract.Interfaces;

namespace BenchMark.PublishBenchmarks
{
    [MemoryDiagnoser]
    public class PublishingWithMiddleware
    {
        [Params("", "Metrics", "OTEL", "Resilience")]
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
                case "Resilience":
                    contractConnection.RegisterResiliencePolicy(
                        retryPolicy: (3, (int cnt) => TimeSpan.FromMilliseconds(100)),
                        circuitBreakPolicy: (3, TimeSpan.FromSeconds(1))
                    );
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