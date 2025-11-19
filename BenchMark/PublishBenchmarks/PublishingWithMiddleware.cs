using BenchmarkDotNet.Attributes;
using MQContract;
using MQContract.Interfaces;

namespace BenchMark.PublishBenchmarks
{
    [MemoryDiagnoser]
    public class PublishingWithMiddleware
    {
        [Params("", "Metrics", "OTEL", "Resilience", "Metrics&OTEL", "Metrics&Resilience", "OTEL&Resilience", "ALL")]
        public string Middleware { get; set; } = string.Empty;

        public const string ChannelName = "sample";
        private const string MessageContent = "The quick brown fox";
        private IContractedConnection? contractConnection;

        [GlobalSetup]
        public void Setup()
        {
            contractConnection = ContractConnection.Instance(new FakePublishConnection());
            if (Middleware.Contains("Metrics")||Equals(Middleware,"ALL"))
                contractConnection.AddMetrics(null, true);
            if (Middleware.Contains("OTEL")||Equals(Middleware, "ALL"))
                contractConnection.EnableOpenTelemetry();
            if (Middleware.Contains("Resilience")||Equals(Middleware, "ALL"))
                contractConnection.RegisterResiliencePolicy(
                        retryPolicy: (3, (int cnt) => TimeSpan.FromMilliseconds(100)),
                        circuitBreakPolicy: (3, TimeSpan.FromSeconds(1))
                    );
        }

        [Benchmark]
        public async Task PublishBasicEncodedMessage()
        {
            _ = await contractConnection!.PublishAsync<string>(MessageContent, channel: ChannelName);
        }
    }
}