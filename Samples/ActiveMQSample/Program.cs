using Messages;
using MQContract;
using MQContract.ActiveMQ;

var serviceConnection = new Connection(new Uri("amqp:tcp://localhost:5672"),"artemis","artemis");

await SampleExecution.ExecuteSample(serviceConnection, "ActiveMQ");
