﻿using Messages;
using MQContract.Redis;
using StackExchange.Redis;
using System.Net;

var conf = new ConfigurationOptions();
conf.EndPoints.Add("localhost");

var serviceConnection = new Connection(conf);

await SampleExecution.ExecuteSample(serviceConnection, "Redis");