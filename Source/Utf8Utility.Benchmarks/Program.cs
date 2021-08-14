using BenchmarkDotNet.Running;

namespace Utf8Utility.Benchmarks;

/// <summary>
/// ベンチマーク
/// </summary>
class Program
{
    static void Main(string[] args)
        => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}