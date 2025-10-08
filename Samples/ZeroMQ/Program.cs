// See https://aka.ms/new-console-template for more information
using Messages;
using MQContract.ZeroMQ;

var serviceConnection = new Connection();
serviceConnection.BindAsServer("tcp://localhost:8080");
serviceConnection.BindInboxAddress("tcp://localhost:8081");
serviceConnection.ConnectToServer("tcp://localhost:8080");

await SampleExecution.ExecuteSample(serviceConnection, "ZeroMQ");