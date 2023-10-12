using BenchmarkDotNet.Attributes;
using Utf8Utility.Benchmarks.Helpers;

namespace Utf8Utility.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class Utf8DictionaryTryAddBenchmark
{
    const int Length = 10;

    Utf8Array[] _keys = null!;

    [Params(1, 10, 100, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _keys = new Utf8Array[Count];

        for (var i = 0; i < _keys.Length; i++)
        {
            _keys[i] = new Utf8Array(StringHelper.GetAsciiRandomString(Length));
        }
    }

    [Benchmark]
    public Dictionary<Utf8Array, int> Dictionary()
    {
        var dict = new Dictionary<Utf8Array, int>();

        foreach (var key in _keys)
        {
            dict.TryAdd(key, 1);
        }

        return dict;
    }

    [Benchmark]
    public Utf8ArrayDictionary<int> Utf8Dictionary()
    {
        var utf8Dict = new Utf8ArrayDictionary<int>();

        foreach (var key in _keys)
        {
            utf8Dict.TryAdd(key, 1);
        }

        return utf8Dict;
    }

    [Benchmark]
    public Dictionary<Utf8Array, int> Dictionary_SetCapacity()
    {
        var dict = new Dictionary<Utf8Array, int>(Count);

        foreach (var key in _keys)
        {
            dict.TryAdd(key, 1);
        }

        return dict;
    }

    [Benchmark]
    public Utf8ArrayDictionary<int> Utf8Dictionary_SetCapacity()
    {
        var utf8Dict = new Utf8ArrayDictionary<int>(Count);

        foreach (var key in _keys)
        {
            utf8Dict.TryAdd(key, 1);
        }

        return utf8Dict;
    }
}
