﻿using System.Collections.Generic;
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
        public Dictionary<Utf8String, int> Dictionary()
        {
            var dict = new Dictionary<Utf8String, int>();

            foreach (var key in Keys)
            {
                dict.TryAdd(key, 1);
            }

            return dict;
        }

        [Benchmark]
        public Utf8StringDictionary<int> Utf8Dictionary()
        {
            var utf8Dict = new Utf8StringDictionary<int>();

            foreach (var key in Keys)
            {
                utf8Dict.Add(key, 1);
            }

            return utf8Dict;
        }
    }
}