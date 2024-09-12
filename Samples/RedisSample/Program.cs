using Messages;
using MQContract.Redis;
using StackExchange.Redis;
using System.Net;

using var sourceCancel = new CancellationTokenSource();

Console.CancelKeyPress += delegate {
    sourceCancel.Cancel();
};

var conf = new ConfigurationOptions();
conf.EndPoints.Add("localhost");

var serviceConnection = new Connection(conf);

await SampleExecution.ExecuteSample(serviceConnection, "Redis", sourceCancel);
