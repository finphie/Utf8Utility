using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Utf8Utility.Benchmarks.Helpers;

namespace Utf8Utility.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [MemoryDiagnoser]
    public class Utf8StringDictionaryAddBenchmark
    {
        const int Length = 10;

        readonly Dictionary<Utf8String, int> _dict = new();
        readonly Utf8StringDictionary<int> _utf8Dict = new();

        [Params(1, 10, 100, 1000)]
        public int Count { get; set; }

        [NotNull]
        public Utf8String[]? Keys { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            Keys = new Utf8String[Count];

            for (var i = 0; i < Keys.Length; i++)
            {
                Keys[i] = new Utf8String(StringHelper.RandomString(Length));
            }
        }

        [Benchmark]
        public void Dictionary()
        {
            foreach (var key in Keys)
            {
                _dict.Add(key, 1);
            }
        }

        [Benchmark]
        public void Utf8Dictionary()
        {
            foreach (var key in Keys)
            {
                _utf8Dict.Add(key, 1);
            }
        }
    }
}