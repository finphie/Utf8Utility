using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using BenchmarkDotNet.Attributes;
using Utf8Utility.Text;

namespace Utf8Utility.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class Utf8GetLengthBenchmark
{
    byte[] _value = null!;

    [Params("a")]
    public string? Value { get; set; }

    [Params(/*10, 63, 64, 95, 96,*/ 10000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new StringBuilder();

        for (var i = 0; i < Count; i++)
        {
            builder.Append(Value);
        }

        _value = Encoding.UTF8.GetBytes(builder.ToString());
    }

    //[Benchmark]
    public int GetLength_Table()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            var length = UnicodeUtility.GetUtf8SequenceLength(start);

            start = ref Unsafe.AddByteOffset(ref start, (nint)(uint)length);
            count++;
        }

        return count;
    }

    //[Benchmark]
    public int GetLength_Byte()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }

    //[Benchmark]
    public int GetLength_PopCount()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        if (_value.Length >= sizeof(ulong))
        {
            const ulong Mask = 0x8080808080808080 >> 7;
            end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong));

            do
            {
                var number = Unsafe.ReadUnaligned<ulong>(ref start);

                var x = ((number >> 6) | (~number >> 7)) & Mask;
                count += BitOperations.PopCount(x);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong));
        }

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }

    //[Benchmark]
    public int GetLength_PopCount_SoftwareFallback()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        if (_value.Length >= sizeof(ulong))
        {
            end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong));

            do
            {
                var number = Unsafe.ReadUnaligned<ulong>(ref start);

                const ulong Mask = 0x8080808080808080 >> 7;
                var x = ((number >> 6) | (~number >> 7)) & Mask;

                const ulong C1 = 0x_55555555_55555555ul;
                const ulong C2 = 0x_33333333_33333333ul;
                const ulong C3 = 0x_0F0F0F0F_0F0F0F0Ful;
                const ulong C4 = 0x_01010101_01010101ul;

                x -= (x >> 1) & C1;
                x = (x & C2) + ((x >> 2) & C2);
                x = (((x + (x >> 4)) & C3) * C4) >> 56;

                count += (int)x;
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong));
        }

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }

    [Benchmark(Baseline = true)]
    public int GetLength_Avx2()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        if (_value.Length >= Vector256<byte>.Count)
        {
            end = ref Unsafe.SubtractByteOffset(ref end, Vector256<byte>.Count);

            do
            {
                var sum = Vector256<byte>.Zero;
                ref var end2 = ref Unsafe.AddByteOffset(ref start, (nint)(uint)Math.Min(255 * 32, Unsafe.ByteOffset(ref start, ref end)));

                do
                {
                    var s = Vector256.LoadUnsafe(ref start);
                    sum = Avx2.Subtract(sum, Avx2.CompareGreaterThan(s.AsSByte(), Vector256.Create<sbyte>(-0x41)).AsByte());
                    start = ref Unsafe.AddByteOffset(ref start, Vector256<byte>.Count);
                }
                while (Unsafe.IsAddressLessThan(ref start, ref end2));

                var sumHigh = Avx2.UnpackHigh(sum, Vector256<byte>.Zero);
                var sumLow = Avx2.UnpackLow(sum, Vector256<byte>.Zero);
                var sum16x16 = Avx2.Add(sumHigh.AsInt16(), sumLow.AsInt16());
                var sum16x8 = Avx2.Add(sum16x16, Avx2.Permute2x128(sum16x16, sum16x16, 1));

                const byte Control = (0 << 6) | (0 << 4) | (2 << 2) | 3;
                var sum16x4 = Avx2.Add(sum16x8, Avx2.Shuffle(sum16x8.AsInt32(), Control).AsInt16());

                var temp = sum16x4.AsInt64().GetElement(0);
                count += (int)((temp >> 0) & 0xffff);
                count += (int)((temp >> 16) & 0xffff);
                count += (int)((temp >> 32) & 0xffff);
                count += (int)((temp >> 48) & 0xffff);
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, Vector256<byte>.Count);
        }

        if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
        {
            end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong));

            do
            {
                var number = Unsafe.ReadUnaligned<ulong>(ref start);

                const ulong Mask = 0x8080808080808080 >> 7;
                var x = ((number >> 6) | (~number >> 7)) & Mask;

                count += BitOperations.PopCount(x);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong));
        }

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }

    [Benchmark]
    public int GetLength_Avx2_2()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        if (_value.Length >= Vector256<byte>.Count)
        {
            end = ref Unsafe.SubtractByteOffset(ref end, Vector256<byte>.Count);
            var i = 0;
            var l = _value.Length / 32 * 32;

            do
            {
                var sum = Vector256<byte>.Zero;
                //ref var end2 = ref Unsafe.AddByteOffset(ref start, Math.Min(255 * 32, Unsafe.ByteOffset(ref start, ref end)));
                var limit = Math.Min(255 * 32, l - i);
                var j = 0;

                for (; j < limit; j += 32)
                {
                    var s = Vector256.LoadUnsafe(ref start, (nuint)(j));
                    sum = Avx2.Subtract(sum, Avx2.CompareGreaterThan(s.AsSByte(), Vector256.Create<sbyte>(-0x41)).AsByte());

                }

                var sumHigh = Avx2.UnpackHigh(sum, Vector256<byte>.Zero);
                var sumLow = Avx2.UnpackLow(sum, Vector256<byte>.Zero);
                var sum16x16 = Avx2.Add(sumHigh.AsInt16(), sumLow.AsInt16());
                var sum16x8 = Avx2.Add(sum16x16, Avx2.Permute2x128(sum16x16, sum16x16, 1));

                const byte Control = (0 << 6) | (0 << 4) | (2 << 2) | 3;
                var sum16x4 = Avx2.Add(sum16x8, Avx2.Shuffle(sum16x8.AsInt32(), Control).AsInt16());

                var temp = sum16x4.AsInt64().GetElement(0);
                count += (int)((temp >> 0) & 0xffff);
                count += (int)((temp >> 16) & 0xffff);
                count += (int)((temp >> 32) & 0xffff);
                count += (int)((temp >> 48) & 0xffff);

                i += j;
                start = ref Unsafe.AddByteOffset(ref start, (nint)(uint)j);
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));


            end = ref Unsafe.AddByteOffset(ref end, Vector256<byte>.Count);
        }

        if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
        {
            end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong));

            do
            {
                var number = Unsafe.ReadUnaligned<ulong>(ref start);

                const ulong Mask = 0x8080808080808080 >> 7;
                var x = ((number >> 6) | (~number >> 7)) & Mask;

                count += BitOperations.PopCount(x);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong));
        }

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }

    [Benchmark]
    public int GetLength_Avx2_3()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        if (_value.Length >= Vector256<byte>.Count)
        {
            end = ref Unsafe.SubtractByteOffset(ref end, Vector256<byte>.Count);
            var i = 0;
            var l = _value.Length / 32 * 32;

            for (; i < l;)
            {
                var sum = Vector256<byte>.Zero;
                //ref var end2 = ref Unsafe.AddByteOffset(ref start, Math.Min(255 * 32, Unsafe.ByteOffset(ref start, ref end)));
                var limit = Math.Min(255 * 32, l - i);
                var j = 0;

                for (; j < limit; j += 32)
                {
                    var s = Vector256.LoadUnsafe(ref start, (nuint)(i + j));
                    sum = Avx2.Subtract(sum, Avx2.CompareGreaterThan(s.AsSByte(), Vector256.Create<sbyte>(-0x41)).AsByte());

                }

                var sumHigh = Avx2.UnpackHigh(sum, Vector256<byte>.Zero);
                var sumLow = Avx2.UnpackLow(sum, Vector256<byte>.Zero);
                var sum16x16 = Avx2.Add(sumHigh.AsInt16(), sumLow.AsInt16());
                var sum16x8 = Avx2.Add(sum16x16, Avx2.Permute2x128(sum16x16, sum16x16, 1));

                const byte Control = (0 << 6) | (0 << 4) | (2 << 2) | 3;
                var sum16x4 = Avx2.Add(sum16x8, Avx2.Shuffle(sum16x8.AsInt32(), Control).AsInt16());

                var temp = sum16x4.AsInt64().GetElement(0);
                count += (int)((temp >> 0) & 0xffff);
                count += (int)((temp >> 16) & 0xffff);
                count += (int)((temp >> 32) & 0xffff);
                count += (int)((temp >> 48) & 0xffff);

                i += j;

            }

            start = ref Unsafe.AddByteOffset(ref start, (nint)(uint)i);
            end = ref Unsafe.AddByteOffset(ref end, Vector256<byte>.Count);
        }

        if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
        {
            end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong));

            do
            {
                var number = Unsafe.ReadUnaligned<ulong>(ref start);

                const ulong Mask = 0x8080808080808080 >> 7;
                var x = ((number >> 6) | (~number >> 7)) & Mask;

                count += BitOperations.PopCount(x);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong));
        }

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }

    [Benchmark]
    public int GetLength_Avx2_4()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        if (_value.Length >= Vector256<byte>.Count)
        {
            end = ref Unsafe.SubtractByteOffset(ref end, Vector256<byte>.Count);

            nuint i = 0;
            var length32 = (nuint)(_value.Length & -Vector256<byte>.Count);

            do
            {
                var sum = Vector256<byte>.Zero;
                var limit = Math.Min((nuint)(255 * Vector256<byte>.Count), length32 - i);
                nuint j = 0;

                for (; j < limit; j += (nuint)Vector256<byte>.Count)
                {
                    var vector = Vector256.LoadUnsafe(ref start, j);
                    sum = Avx2.Subtract(sum, Avx2.CompareGreaterThan(vector.AsSByte(), Vector256.Create<sbyte>(-0x41)).AsByte());
                }

                var sumHigh = Avx2.UnpackHigh(sum, Vector256<byte>.Zero);
                var sumLow = Avx2.UnpackLow(sum, Vector256<byte>.Zero);
                var sum16x16 = Avx2.Add(sumHigh.AsInt16(), sumLow.AsInt16());
                var sum16x8 = Avx2.Add(sum16x16, Avx2.Permute2x128(sum16x16, sum16x16, 1));

                const byte Control = (0 << 6) | (0 << 4) | (2 << 2) | 3;
                var sum16x4 = Avx2.Add(sum16x8, Avx2.Shuffle(sum16x8.AsInt32(), Control).AsInt16());

                var temp = sum16x4.AsInt64().GetElement(0);
                count += (int)((temp >> 0) & 0xffff);
                count += (int)((temp >> 16) & 0xffff);
                count += (int)((temp >> 32) & 0xffff);
                count += (int)((temp >> 48) & 0xffff);

                i += j;
                start = ref Unsafe.AddByteOffset(ref start, j);
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));


            end = ref Unsafe.AddByteOffset(ref end, Vector256<byte>.Count);
        }

        if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
        {
            end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong));

            do
            {
                var number = Unsafe.ReadUnaligned<ulong>(ref start);

                const ulong Mask = 0x8080808080808080 >> 7;
                var x = ((number >> 6) | (~number >> 7)) & Mask;

                count += BitOperations.PopCount(x);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong));
        }

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }

    [Benchmark]
    public int GetLength_Avx2_5()
    {
        var count = 0;

        ref var start = ref MemoryMarshal.GetArrayDataReference(_value);
        ref var end = ref Unsafe.AddByteOffset(ref start, (nint)(uint)_value.Length);

        if (_value.Length >= Vector256<byte>.Count)
        {
            nuint i = 0;
            var length32 = (nuint)(_value.Length & -Vector256<byte>.Count);

            do
            {
                var sum = Vector256<byte>.Zero;
                var limit = Math.Min((nuint)(255 * Vector256<byte>.Count), length32 - i);
                nuint j = 0;

                for (; j < limit; j += (nuint)Vector256<byte>.Count)
                {
                    var vector = Vector256.LoadUnsafe(ref start, j);
                    sum = Avx2.Subtract(sum, Avx2.CompareGreaterThan(vector.AsSByte(), Vector256.Create<sbyte>(-0x41)).AsByte());
                }

                var sumHigh = Avx2.UnpackHigh(sum, Vector256<byte>.Zero);
                var sumLow = Avx2.UnpackLow(sum, Vector256<byte>.Zero);
                var sum16x16 = Avx2.Add(sumHigh.AsInt16(), sumLow.AsInt16());
                var sum16x8 = Avx2.Add(sum16x16, Avx2.Permute2x128(sum16x16, sum16x16, 1));

                const byte Control = (0 << 6) | (0 << 4) | (2 << 2) | 3;
                var sum16x4 = Avx2.Add(sum16x8, Avx2.Shuffle(sum16x8.AsInt32(), Control).AsInt16());

                var temp = sum16x4.AsInt64().GetElement(0);
                count += (int)((temp >> 0) & 0xffff);
                count += (int)((temp >> 16) & 0xffff);
                count += (int)((temp >> 32) & 0xffff);
                count += (int)((temp >> 48) & 0xffff);

                i += j;
            }
            while (i < length32);

            start = ref Unsafe.AddByteOffset(ref start, i);
        }

        if (Unsafe.ByteOffset(ref start, ref end) >= sizeof(ulong))
        {
            end = ref Unsafe.SubtractByteOffset(ref end, sizeof(ulong));

            do
            {
                var number = Unsafe.ReadUnaligned<ulong>(ref start);

                const ulong Mask = 0x8080808080808080 >> 7;
                var x = ((number >> 6) | (~number >> 7)) & Mask;

                count += BitOperations.PopCount(x);
                start = ref Unsafe.AddByteOffset(ref start, sizeof(ulong));
            }
            while (!Unsafe.IsAddressGreaterThan(ref start, ref end));

            end = ref Unsafe.AddByteOffset(ref end, sizeof(ulong));
        }

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            if ((start & 0xC0) != 0x80)
            {
                count++;
            }

            start = ref Unsafe.AddByteOffset(ref start, 1);
        }

        return count;
    }
}
