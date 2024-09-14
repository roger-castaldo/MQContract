using Messages;
using MQContract;
using MQContract.KubeMQ;
using MQContract.KubeMQ.Options;

await using var serviceConnection = new Connection(new ConnectionOptions()
{
    Logger=new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider().CreateLogger("Messages"),
    ClientId="KubeMQSample"
})
    .RegisterStoredChannel("StoredArrivals");

await SampleExecution.ExecuteSample(serviceConnection, "KubeMQ");