using DotPulsar;
using Messages;
using MQContract.ApachePulsar;

#pragma warning disable S1075 // URIs should not be hardcoded
//This is a sample program with a localhost connection so this is necessary
var builder = PulsarClient.Builder()
    .ServiceUrl(new("pulsar://localhost:6650"));
#pragma warning restore S1075 // URIs should not be hardcoded

var serviceConnection = new Connection(builder!);

await SampleExecution.ExecuteSample(serviceConnection, "ApachePulsar");
