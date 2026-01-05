// See https://aka.ms/new-console-template for more information
using BenchMark.InMemoryBenchmarks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run(
    typeof(SubscribingInMemory).Assembly, // all benchmarks from given assembly are going to be executed
    ManualConfig
        .Create(
            DefaultConfig.Instance
            .AddJob(
                BenchmarkDotNet.Jobs.Job.Default
                .WithWarmupCount(5)
                .WithMinIterationCount(3)
                .WithIterationCount(32)
                .WithInvocationCount(16)
                .WithMaxIterationCount(16)
            )
            .WithOptions(ConfigOptions.DisableLogFile)
        ),
    args
);