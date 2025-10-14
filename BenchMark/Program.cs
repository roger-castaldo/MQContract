// See https://aka.ms/new-console-template for more information
using BenchMark.PublishBenchmarks;
using BenchmarkDotNet.Running;

_ = BenchmarkRunner.Run<Publishing>();
_ = BenchmarkRunner.Run<PublishingWithMiddleware>();
