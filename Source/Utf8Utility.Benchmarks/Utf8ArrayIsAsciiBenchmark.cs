using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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
    public bool IsAscii_Byte()
    {
        for (nuint index = 0; (int)index < _value.ByteCount; index++)
        {
            if ((sbyte)Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index) < 0)
            {
                return false;
            }
        }

        return true;
    }

    [Benchmark]
    public bool IsAscii_ULong()
    {
        nuint index = 0;
        var mask1 = 0UL;

        while ((int)index < _value.ByteCount - sizeof(ulong))
        {
            var value = Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index));

            mask1 |= value;
            index += sizeof(ulong);
        }

        byte mask2 = 0;

        while ((int)index < _value.ByteCount)
        {
            mask2 |= Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index);
            index++;
        }

        return ((mask1 | mask2) & 0x8080808080808080) == 0;
    }

    [Benchmark]
    public bool IsAscii_Ulong4()
    {
        nuint index = 0;
        var mask1 = 0UL;
        var mask2 = 0UL;
        var mask3 = 0UL;
        var mask4 = 0UL;

        while ((int)index < _value.ByteCount - (sizeof(ulong) * 4))
        {
            ref var valueStart = ref Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index));
            mask1 |= valueStart;
            mask2 |= Unsafe.Add(ref valueStart, 1);
            mask3 |= Unsafe.Add(ref valueStart, 2);
            mask4 |= Unsafe.Add(ref valueStart, 3);

            index += sizeof(ulong) * 4;
        }

        while ((int)index < _value.ByteCount - sizeof(ulong))
        {
            var value = Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index));

            mask1 |= value;
            index += sizeof(ulong);
        }

        byte mask5 = 0;

        while ((int)index < _value.ByteCount)
        {
            mask5 |= Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index);
            index++;
        }

        return ((mask1 | mask2 | mask3 | mask4 | mask5) & 0x8080808080808080) == 0;
    }

    [Benchmark]
    public unsafe bool IsAscii_Sse2()
    {
        var index = 0;
        var mask1 = Vector128<byte>.Zero;

        if (_value.ByteCount >= 16)
        {
            fixed (byte* ptr = &_value.DangerousGetReference())
            {
                for (; index <= _value.ByteCount - 16; index += 16)
                {
                    mask1 = Sse2.Or(mask1, Sse2.LoadVector128(ptr + index));
                }
            }
        }

        var result = Sse2.MoveMask(mask1);
        sbyte mask2 = 0;

        for (; index < _value.ByteCount; index++)
        {
            mask2 |= (sbyte)Unsafe.AddByteOffset(ref _value.DangerousGetReference(), (nint)(uint)index);
        }

        result |= mask2 & 0x80;

        return result == 0;
    }

    [Benchmark]
    public unsafe bool IsAscii_Avx2()
    {
        nuint index = 0;
        var mask1 = Vector256<byte>.Zero;

        if (_value.ByteCount >= Vector256<byte>.Count)
        {
            var endIndex = _value.ByteCount - Vector256<byte>.Count;
            ref var first = ref Unsafe.As<byte, Vector256<byte>>(ref _value.DangerousGetReference());

            do
            {
                mask1 = Avx2.Or(mask1, Unsafe.AddByteOffset(ref first, index));
                index += (uint)Vector256<byte>.Count;
            }
            while ((int)index <= endIndex);
        }

        var result = Avx2.MoveMask(mask1);
        var mask2 = 0;

        for (; (int)index < _value.ByteCount; index++)
        {
            mask2 |= Unsafe.AddByteOffset(ref _value.DangerousGetReference(), index);
        }

        result |= mask2 & 0x80;

        return result == 0;
    }
}
