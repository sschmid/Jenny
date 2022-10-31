using BenchmarkDotNet.Running;

namespace Jenny.Benchmarks
{
    static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(CodeGeneratorDataBenchmarks));
        }
    }
}
