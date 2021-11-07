using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Utf8Utility.Benchmarks;

[Config(typeof(BenchmarkConfig)]
public class Utf8ArrayGetLengthBenchmark
{
    Utf8Array _value;

    [Params("abcd", "あいうえお", "あaαβaあααいうazzαああαabc")]
    public string? Value { get; set; }

    [Params(1, 32, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup() => _value = new(string.Concat(Enumerable.Repeat(Value, Count)));

    [Benchmark]
    public int GetLength_Loop()
    {
        var count = 0;
        nuint index = 0;

        while ((int)index < _value.ByteCount)
        {
            var value = Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index);

            if ((value & 0xC0) != 0x80)
            {
                count++;
            }

            index++;
        }

        return count;
    }

    [Benchmark]
    public int GetLength_Table() => _value.GetLength();

    [Benchmark]
    public int GetLength_Long()
    {
        const ulong Mask = 0x8080808080808080 >> 7;

        var count = 0;
        nuint index = 0;
        var length = _value.ByteCount - sizeof(ulong);

        while ((int)index <= length)
        {
            var value = Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index));
            var x = ((value >> 6) | (~value >> 7)) & Mask;
            count += BitOperations.PopCount(x);
            index += sizeof(ulong);
        }

        while ((int)index < _value.ByteCount)
        {
            var value = Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index);

            if ((value & 0xC0) != 0x80)
            {
                count++;
            }

            index++;
        }

        return count;
    }
}
