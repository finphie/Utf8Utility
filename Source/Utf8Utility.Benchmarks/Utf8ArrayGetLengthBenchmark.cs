using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Utf8Utility.Benchmarks.Helpers;

namespace Utf8Utility.Benchmarks;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class Utf8ArrayGetLengthBenchmark
{
    Utf8Array _value;

    [Params("aα𩸽あbaα𩸽あbaα𩸽あbaα𩸽あbaα𩸽あb")]
    public string? Value { get; set; }

    [GlobalSetup]
    public void Setup() => _value = new(Value!);

    [Benchmark]
    public int GetLength_Table() => _value.GetLength();

    [Benchmark]
    public int GetLength_Long()
    {
        const ulong Mask = 0x8080808080808080;

        var count = 0;
        nuint i = 0;
        var length = _value.ByteCount - 8;

        while ((int)i <= length)
        {
            var value = Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref _value.DangerousGetReference(), i));
            var x = ((value >> 6) | (~value >> 7)) & (Mask >> 7);
            count += System.Numerics.BitOperations.PopCount(x);
            i += 8;
        }

        while ((int)i < _value.ByteCount)
        {
            var value = Unsafe.AddByteOffset(ref _value.DangerousGetReference(), i);

            if ((value & 0xC0) != 0x80)
            {
                count++;
            }

            i++;
        }

        return count;
    }
}
