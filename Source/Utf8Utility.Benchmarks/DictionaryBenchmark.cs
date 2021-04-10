using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Utf8Utility.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [MemoryDiagnoser]
    public class DictionaryBenchmark
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
                var key = new Utf8String(RandomString(Length));

                _dict.Add(key, 1);
                _utf8Dict.Add(key, 1);
            }

            Key = new Utf8String(RandomString(Length));
            _dict.Add(Key, 1);
            _utf8Dict.Add(Key, 1);
        }

        [Benchmark]
        public int Dictionary_TryGetValue()
        {
            _dict.TryGetValue(Key, out var value);
            return value;
        }

        [Benchmark]
        public int Utf8Dictionary_TryGetValue()
        {
            _utf8Dict.TryGetValue(Key, out var value);
            return value;
        }

        [Benchmark]
        public int Utf8Dictionary_TryGetValue_Span()
        {
            _utf8Dict.TryGetValue(Key.AsSpan(), out var value);
            return value;
        }

        static string RandomString(int length)
        {
            var random = new Random();
            const string Table = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var array = Enumerable.Repeat(Table, length)
                .Select(x => x[random.Next(x.Length)])
                .ToArray();
            return new string(array);
        }
    }
}