using Messages;
using MQContract.RabbitMQ;
using RabbitMQ.Client;

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName="guest",
    Password="guest",
    MaxMessageSize=1024*1024*4
};

var serviceConnection = new Connection(factory)
    .ExchangeDeclare("Greeting", ExchangeType.Fanout)
    .ExchangeDeclare("Greeting.Response", ExchangeType.Fanout)
    .ExchangeDeclare("StoredArrivals", ExchangeType.Fanout,true)
    .ExchangeDeclare("Arrivals", ExchangeType.Fanout);

await SampleExecution.ExecuteSample(serviceConnection, "RabbitMQ");