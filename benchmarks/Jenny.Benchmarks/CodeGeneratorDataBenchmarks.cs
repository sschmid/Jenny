using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Jenny.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class CodeGeneratorDataBenchmarks
    {
        CodeGeneratorData _data;
        string _template;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _data = new CodeGeneratorData();
            for (var i = 0; i < 1000; i++)
                _data[$"key{i}"] = $"value{i}";

            _template = @"test-${key123}-test";
        }

        [Benchmark]
        public void ReplacePlaceholders()
        {
            _data.ReplacePlaceholders(_template);
        }
    }
}
