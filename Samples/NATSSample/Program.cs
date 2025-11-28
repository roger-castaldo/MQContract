using Messages;
using MQContract;
using MQContract.NATS;
using NATS.Client.JetStream.Models;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


var serviceName = "MQContract";

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(serviceName)  // Tracks activities from this source
    .SetResourceBuilder(OpenTelemetry.Resources.ResourceBuilder.CreateDefault().AddService(serviceName))
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new("http://localhost:4317");
    })     // Optional: Export to OTLP endpoint
    .Build();


var serviceConnection = new Connection(new NATS.Client.Core.NatsOpts()
{
    LoggerFactory=new Microsoft.Extensions.Logging.LoggerFactory(),
    Name="NATSSample"
});

var streamConfig = new StreamConfig("StoredArrivalsStream", ["StoredArrivals"])
{
    MaxBytes = 100 * 1024 * 1024
};
await serviceConnection.CreateStreamAsync(streamConfig);

var mapper = new ChannelMapper()
    .AddPublishSubscriptionMap("StoredArrivals", "StoredArrivalsStream");

await SampleExecution.ExecuteSample(serviceConnection, "NatsIO", mapper);