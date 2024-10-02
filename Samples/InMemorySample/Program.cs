using Messages;
using MQContract.InMemory;

var serviceConnection = new Connection();

await SampleExecution.ExecuteSample(serviceConnection, "InMemory");