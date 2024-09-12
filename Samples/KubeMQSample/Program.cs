using Messages;
using MQContract;
using MQContract.KubeMQ;
using MQContract.KubeMQ.Options;

using var sourceCancel = new CancellationTokenSource();

Console.CancelKeyPress += delegate {
    sourceCancel.Cancel();
};

await using var serviceConnection = new Connection(new ConnectionOptions()
{
    Logger=new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider().CreateLogger("Messages"),
    ClientId="KubeMQSample"
})
    .RegisterStoredChannel("StoredArrivals");

await SampleExecution.ExecuteSample(serviceConnection, "KubeMQ", sourceCancel);