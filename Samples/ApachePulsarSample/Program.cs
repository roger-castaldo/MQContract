using DotPulsar;
using Messages;
using MQContract;
using MQContract.ApachePulsar;

var builder = PulsarClient.Builder()
    .ServiceUrl(new("pulsar://localhost:6650"));

var serviceConnection = new Connection(builder!);

await SampleExecution.ExecuteSample(serviceConnection, "ApachePulsar");
