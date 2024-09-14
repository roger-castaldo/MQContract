using Messages;
using MQContract;
using MQContract.NATS;
using NATS.Client.JetStream.Models;

var serviceConnection = new Connection(new NATS.Client.Core.NatsOpts()
{
    LoggerFactory=new Microsoft.Extensions.Logging.LoggerFactory(),
    Name="NATSSample"
});

var streamConfig = new StreamConfig("StoredArrivalsStream", ["StoredArrivals"]);
await serviceConnection.CreateStreamAsync(streamConfig);

var mapper = new ChannelMapper()
    .AddPublishSubscriptionMap("StoredArrivals", "StoredArrivalsStream");

await SampleExecution.ExecuteSample(serviceConnection, "NatsIO", mapper);