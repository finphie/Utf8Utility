using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Utf8Utility.Benchmarks.Helpers;

namespace Utf8Utility.Benchmarks;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class Utf8ArrayDictionaryTryAddBenchmark
{
    const int Length = 10;

    [Params(1, 10, 100, 1000)]
    public int Count { get; set; }

    [NotNull]
    public Utf8Array[]? Keys { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Keys = new Utf8Array[Count];

        for (var i = 0; i < Keys.Length; i++)
        {
            Keys[i] = new Utf8Array(StringHelper.RandomString(Length));
        }
    }

    [Benchmark]
    public Dictionary<Utf8Array, int> Dictionary()
    {
        var dict = new Dictionary<Utf8Array, int>();

        foreach (var key in Keys)
        {
            dict.TryAdd(key, 1);
        }

        return dict;
    }

    [Benchmark]
    public Utf8ArrayDictionary<int> Utf8Dictionary()
    {
        var utf8Dict = new Utf8ArrayDictionary<int>();

        foreach (var key in Keys)
        {
            utf8Dict.TryAdd(key, 1);
        }

        return utf8Dict;
    }

    [Benchmark]
    public Dictionary<Utf8Array, int> Dictionary_SetCapacity()
    {
        var dict = new Dictionary<Utf8Array, int>(Count);

        foreach (var key in Keys)
        {
            dict.TryAdd(key, 1);
        }

        return dict;
    }

    [Benchmark]
    public Utf8ArrayDictionary<int> Utf8Dictionary_SetCapacity()
    {
        var utf8Dict = new Utf8ArrayDictionary<int>(Count);

        foreach (var key in Keys)
        {
            utf8Dict.TryAdd(key, 1);
        }

        return utf8Dict;
    }
}
