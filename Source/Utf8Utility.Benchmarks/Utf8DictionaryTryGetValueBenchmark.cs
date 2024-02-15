using System.Collections.Frozen;
using BenchmarkDotNet.Attributes;
using Utf8Utility.Benchmarks.Helpers;

namespace Utf8Utility.Benchmarks;

public class Utf8DictionaryTryGetValueBenchmark
{
    const int Length = 10;

    readonly Dictionary<Utf8Array, int> _dict = [];
    readonly Utf8ArrayDictionary<int> _utf8Dict = new();

    FrozenDictionary<Utf8Array, int> _frozenDictionary;

    [Params(1, 10, 100, 1000)]
    public int Count { get; set; }

    public Utf8Array Key { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < Count; i++)
        {
            var key = new Utf8Array(StringHelper.GetAsciiRandomString(Length));

            _dict.Add(key, 1);
            _utf8Dict.TryAdd(key, 1);
        }

        Key = new Utf8Array(StringHelper.GetAsciiRandomString(Length));
        _dict.Add(Key, 1);
        _frozenDictionary = _dict.ToFrozenDictionary();
        _utf8Dict.TryAdd(Key, 1);
    }

    [Benchmark]
    public int Dictionary()
    {
        _dict.TryGetValue(Key, out var value);
        return value;
    }

    [Benchmark]
    public int FrozenDictionary()
    {
        _frozenDictionary.TryGetValue(Key, out var value);
        return value;
    }

    [Benchmark]
    public int Utf8Dictionary()
    {
        _utf8Dict.TryGetValue(Key, out var value);
        return value;
    }

    [Benchmark]
    public int Utf8Dictionary_Span()
    {
        _utf8Dict.TryGetValue(Key.AsSpan(), out var value);
        return value;
    }
}
