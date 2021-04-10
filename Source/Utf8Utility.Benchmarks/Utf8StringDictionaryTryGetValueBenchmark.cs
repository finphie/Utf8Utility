using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Utf8Utility.Benchmarks.Helpers;

namespace Utf8Utility.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [MemoryDiagnoser]
    public class Utf8StringDictionaryTryGetValueBenchmark
    {
        const int Length = 10;

        readonly Dictionary<Utf8String, int> _dict = new();
        readonly Utf8StringDictionary<int> _utf8Dict = new();

        [Params(1, 10, 100, 1000)]
        public int Count { get; set; }

        public Utf8String Key { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            for (var i = 0; i < Count; i++)
            {
                var key = new Utf8String(StringHelper.RandomString(Length));

                _dict.Add(key, 1);
                _utf8Dict.Add(key, 1);
            }

            Key = new Utf8String(StringHelper.RandomString(Length));
            _dict.Add(Key, 1);
            _utf8Dict.Add(Key, 1);
        }

        [Benchmark]
        public int Dictionary()
        {
            _dict.TryGetValue(Key, out var value);
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
}