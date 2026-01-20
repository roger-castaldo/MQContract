using BenchmarkDotNet.Attributes;
using MQContract;

namespace BenchMark.PublishBenchmarks
{
    [MemoryDiagnoser]
    public class PublishingWithMiddleware
    {

        public const string ChannelName = "sample";
        private const string MessageContent = "The quick brown fox";

        [Benchmark(Baseline = true)]
        public async Task PublishWithNoMiddleware()
            => await PublishBasicEncodedMessageMultipleTimes(string.Empty);

        [Benchmark()]
        public async Task PublishWithMetrics()
            => await PublishBasicEncodedMessageMultipleTimes("Metrics");

        [Benchmark()]
        public async Task PublishWithOtel()
            => await PublishBasicEncodedMessageMultipleTimes("OTEL");

        [Benchmark()]
        public async Task PublishWithResilience()
            => await PublishBasicEncodedMessageMultipleTimes("Resilience");

        [Benchmark()]
        public async Task PublishWithMetricsAndOtel()
            => await PublishBasicEncodedMessageMultipleTimes("Metrics&OTEL");

        [Benchmark()]
        public async Task PublishWithMetricsAndResilience()
            => await PublishBasicEncodedMessageMultipleTimes("Metrics&Resilience");

        [Benchmark()]
        public async Task PublishWithOtelAndResilience()
            => await PublishBasicEncodedMessageMultipleTimes("OTEL&Resilience");

        [Benchmark()]
        public async Task PublishWithAll()
            => await PublishBasicEncodedMessageMultipleTimes("ALL");

        private static async Task PublishBasicEncodedMessageMultipleTimes(string middleware)
        {
            await using var contractConnection = ContractConnection.Instance(new FakePublishConnection());
            if (middleware.Contains("Metrics")||Equals(middleware, "ALL"))
                contractConnection.AddMetrics(null, true);
            if (middleware.Contains("OTEL")||Equals(middleware, "ALL"))
                contractConnection.EnableOpenTelemetry();
            if (middleware.Contains("Resilience")||Equals(middleware, "ALL"))
                contractConnection.RegisterResiliencePolicy(
                        retryPolicy: (3, (int cnt) => TimeSpan.FromMilliseconds(100)),
                        circuitBreakPolicy: (3, TimeSpan.FromSeconds(1))
                    );
            for (var x = 0; x<Constants.PublishCount/10; x++)
                _ = await contractConnection!.PublishAsync<string>(MessageContent, channel: ChannelName);
        }
    }
}