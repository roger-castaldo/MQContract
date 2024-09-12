using Messages;
using MQContract;
using MQContract.ActiveMQ;

using var sourceCancel = new CancellationTokenSource();

Console.CancelKeyPress += delegate {
    sourceCancel.Cancel();
};

var serviceConnection = new Connection(new Uri("amqp:tcp://localhost:5672"),"artemis","artemis");

await SampleExecution.ExecuteSample(serviceConnection, "ActiveMQ", sourceCancel);
