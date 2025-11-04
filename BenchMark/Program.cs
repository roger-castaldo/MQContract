// See https://aka.ms/new-console-template for more information
using BenchMark.InMemoryBenchmarks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run(
    typeof(SubscribingInMemory).Assembly, // all benchmarks from given assembly are going to be executed
    ManualConfig
                .Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableLogFile));