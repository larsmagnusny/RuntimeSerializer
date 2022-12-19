// See https://aka.ms/new-console-template for more information

using Benchmark;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<SerializeBenchmark>();