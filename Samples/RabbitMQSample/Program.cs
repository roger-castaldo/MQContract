using Messages;
using MQContract.RabbitMQ;
using RabbitMQ.Client;

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName="guest",
    Password="guest",
    MaxInboundMessageBodySize=1024*1024*4
};

var serviceConnection = new Connection(factory);
await serviceConnection.ExchangeDeclareAsync("Greeting", ExchangeType.Fanout);
await serviceConnection.ExchangeDeclareAsync("StoredArrivals", ExchangeType.Fanout, true);
await serviceConnection.ExchangeDeclareAsync("Arrivals", ExchangeType.Fanout);

await SampleExecution.ExecuteSample(serviceConnection, "RabbitMQ");