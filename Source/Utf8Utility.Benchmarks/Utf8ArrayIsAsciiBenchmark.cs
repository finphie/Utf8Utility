using BenchmarkDotNet.Attributes;
using Utf8Utility.Benchmarks.Helpers;

namespace Utf8Utility.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class Utf8ArrayIsAsciiBenchmark
{
    Utf8Array _value;

    [Params(4, 16, 100)]
    public int Length { get; set; }

    [GlobalSetup]
    public void Setup() => _value = new(StringHelper.GetAsciiRandomString(Length));

    [Benchmark]
    public void IsAscii() => _value.IsAscii();

    [Benchmark]
    public void IsAscii2() => _value.IsAscii2();

    [Benchmark]
    public void IsAscii5() => _value.IsAscii5();

    [Benchmark]
    public void IsAsciiAvx() => _value.IsAsciiAvx();

    [Benchmark]
    public void IsAsciiSse2() => _value.IsAsciiSse2();
}
