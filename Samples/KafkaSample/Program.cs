using Messages;
using MQContract;
using MQContract.Kafka;

using var sourceCancel = new CancellationTokenSource();

Console.CancelKeyPress += delegate {
    sourceCancel.Cancel();
};

await using var serviceConnection = new Connection(new Confluent.Kafka.ClientConfig()
{
    ClientId="KafkaSample",
    BootstrapServers="localhost:56497"
});

await SampleExecution.ExecuteSample(serviceConnection, "Kafka", sourceCancel);