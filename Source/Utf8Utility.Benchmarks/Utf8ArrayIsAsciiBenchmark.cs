using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.Toolkit.HighPerformance;
using Utf8Utility.Benchmarks.Helpers;

namespace Utf8Utility.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class Utf8ArrayIsAsciiBenchmark
{
#nullable disable
    byte[] _value;
#nullable restore

    [Params(4, 16, 100)]
    public int Length { get; set; }

    [GlobalSetup]
    public void Setup()
        => _value = Encoding.ASCII.GetBytes(StringHelper.GetAsciiRandomString(Length));

    [Benchmark]
    public bool IsAscii_Byte()
    {
        for (nuint index = 0; (int)index < _value.Length; index++)
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
        var mask8 = 0UL;

        ref var first = ref _value.DangerousGetReference();

        if (_value.Length - (int)index >= sizeof(ulong))
        {
            ref var current = ref Unsafe.As<byte, ulong>(ref first);
            var endIndex = _value.Length - sizeof(ulong);

            do
            {
                mask8 |= Unsafe.AddByteOffset(ref current, index);
                index += sizeof(ulong);
            }
            while ((int)index < endIndex);
        }

        if (_value.Length - (int)index >= sizeof(uint))
        {
            mask8 |= Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(uint);
        }

        if (_value.Length - (int)index >= sizeof(ushort))
        {
            mask8 |= Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ushort);
        }

        if ((int)index < _value.Length)
        {
            mask8 |= Unsafe.AddByteOffset(ref first, index);
        }

        return (mask8 & 0x8080808080808080) == 0;
    }

    [Benchmark]
    public bool IsAscii_Ulong4()
    {
        nuint index = 0;
        var maskA = 0UL;
        var maskB = 0UL;
        var maskC = 0UL;
        var maskD = 0UL;

        ref var first = ref _value.DangerousGetReference();

        if (_value.Length >= sizeof(ulong) * 4)
        {
            ref var current = ref Unsafe.As<byte, ulong>(ref first);
            var endIndex = _value.Length - (sizeof(ulong) * 4);

            do
            {
                maskA |= Unsafe.AddByteOffset(ref current, index);
                maskB |= Unsafe.AddByteOffset(ref current, index + sizeof(ulong));
                maskC |= Unsafe.AddByteOffset(ref current, index + (sizeof(ulong) * 2));
                maskD |= Unsafe.AddByteOffset(ref current, index + (sizeof(ulong) * 3));
                index += sizeof(ulong) * 4;
            }
            while ((int)index < endIndex);
        }

        if (_value.Length - (int)index >= sizeof(ulong) * 2)
        {
            ref var current = ref Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));

            maskA |= current;
            maskA |= Unsafe.Add(ref current, 1);
            index += sizeof(ulong) * 2;
        }

        if (_value.Length - (int)index >= sizeof(ulong))
        {
            maskA |= Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ulong);
        }

        if (_value.Length - (int)index >= sizeof(uint))
        {
            maskA |= Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(uint);
        }

        if (_value.Length - (int)index >= sizeof(ushort))
        {
            maskA |= Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ushort);
        }

        if ((int)index < _value.Length)
        {
            maskA |= Unsafe.AddByteOffset(ref first, index);
        }

        return ((maskA | maskB | maskC | maskD) & 0x8080808080808080) == 0;
    }

    [Benchmark]
    public unsafe bool IsAscii_Sse2()
    {
        nuint index = 0;
        var mask16 = Vector128<byte>.Zero;

        ref var first = ref _value.DangerousGetReference();

        if (_value.Length >= Vector128<byte>.Count)
        {
            var endIndex = _value.Length - Vector128<byte>.Count;
            ref var current = ref Unsafe.As<byte, Vector128<byte>>(ref first);

            do
            {
                mask16 = Sse2.Or(mask16, Unsafe.AddByteOffset(ref current, index));
                index += (uint)Vector128<byte>.Count;
            }
            while ((int)index <= endIndex);
        }

        var result = Sse2.MoveMask(mask16);
        var mask8 = 0UL;

        if (_value.Length - (int)index >= sizeof(ulong))
        {
            mask8 |= Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ulong);
        }

        if (_value.Length - (int)index >= sizeof(uint))
        {
            mask8 |= Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(uint);
        }

        if (_value.Length - (int)index >= sizeof(ushort))
        {
            mask8 |= Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ushort);
        }

        if ((int)index < _value.Length)
        {
            mask8 |= Unsafe.AddByteOffset(ref first, index);
        }

        return ((uint)result | (mask8 & 0x8080808080808080)) == 0;
    }

    [Benchmark]
    public unsafe bool IsAscii_Avx2()
    {
        nuint index = 0;
        var mask32 = Vector256<byte>.Zero;

        ref var first = ref _value.DangerousGetReference();

        if (_value.Length >= Vector256<byte>.Count)
        {
            var endIndex = _value.Length - Vector256<byte>.Count;
            ref var current = ref Unsafe.As<byte, Vector256<byte>>(ref first);

            do
            {
                mask32 = Avx2.Or(mask32, Unsafe.AddByteOffset(ref current, index));
                index += (uint)Vector256<byte>.Count;
            }
            while ((int)index <= endIndex);
        }

        var result = Avx2.MoveMask(mask32);
        var mask8 = 0UL;

        if (_value.Length - (int)index >= sizeof(ulong) * 2)
        {
            ref var current = ref Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));

            mask8 = current;
            mask8 |= Unsafe.Add(ref current, 1);
            index += sizeof(ulong) * 2;
        }

        if (_value.Length - (int)index >= sizeof(ulong))
        {
            mask8 |= Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ulong);
        }

        if (_value.Length - (int)index >= sizeof(uint))
        {
            mask8 |= Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(uint);
        }

        if (_value.Length - (int)index >= sizeof(ushort))
        {
            mask8 |= Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref first, index));
            index += sizeof(ushort);
        }

        if ((int)index < _value.Length)
        {
            mask8 |= Unsafe.AddByteOffset(ref first, index);
        }

        return ((uint)result | (mask8 & 0x8080808080808080)) == 0;
    }
}
