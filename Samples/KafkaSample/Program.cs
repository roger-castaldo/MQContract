using Messages;
using MQContract.Kafka;

var serviceConnection = new Connection(new Confluent.Kafka.ClientConfig()
{
    ClientId="KafkaSample",
    BootstrapServers="localhost:56497"
});

await SampleExecution.ExecuteSample(serviceConnection, "Kafka");