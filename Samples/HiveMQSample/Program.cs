using HiveMQtt.Client.Options;
using Messages;
using MQContract.HiveMQ;

var serviceConnection = new Connection(new HiveMQClientOptions
{
    Host = "127.0.0.1",
    Port = 1883,
    CleanStart = false,  // <--- Set to false to receive messages queued on the broker
    ClientId = "HiveMQSample"
});

await SampleExecution.ExecuteSample(serviceConnection, "HiveMQ");
