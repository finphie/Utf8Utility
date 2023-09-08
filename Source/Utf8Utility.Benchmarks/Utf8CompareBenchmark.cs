using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using Utf8Utility.Text;

namespace Utf8Utility.Benchmarks;

public class Utf8CompareBenchmark
{
    byte[] _value1 = null!;
    byte[] _value2 = null!;

    [Params(1000)]
    public int Length { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var builder = new StringBuilder();

        for (var i = 0; i < Length; i++)
        {
            builder.Append("あいうえおαβabcdefg𩸽😀🖳");
        }

        _value1 = Encoding.UTF8.GetBytes(builder.ToString() + "a");
        _value2 = Encoding.UTF8.GetBytes(builder.ToString() + "b");
    }

    [Benchmark]
    public int Compare1()
    {
        Unsafe.SkipInit(out nint xBuffer);
        Unsafe.SkipInit(out nint yBuffer);

        ref var xBufferStart = ref Unsafe.As<nint, char>(ref xBuffer);
        ref var yBufferStart = ref Unsafe.As<nint, char>(ref yBuffer);

        ref var xStart = ref MemoryMarshal.GetArrayDataReference(_value1);
        ref var yStart = ref MemoryMarshal.GetArrayDataReference(_value2);

        ref var xEnd = ref Unsafe.AddByteOffset(ref xStart, (nint)(uint)_value1.Length);
        ref var yEnd = ref Unsafe.AddByteOffset(ref yStart, (nint)(uint)_value2.Length);

        do
        {
            WriteUtf16Span(ref xStart, out var xBytesConsumed, ref xBufferStart, out var charsWritten);
            var xSpan = MemoryMarshal.CreateReadOnlySpan(ref xBufferStart, charsWritten);

            WriteUtf16Span(ref yStart, out var yBytesConsumed, ref yBufferStart, out charsWritten);
            var ySpan = MemoryMarshal.CreateReadOnlySpan(ref yBufferStart, charsWritten);

            var result = xSpan.CompareTo(ySpan, StringComparison.OrdinalIgnoreCase);

            if (result != 0)
            {
                return result;
            }

            xStart = ref Unsafe.AddByteOffset(ref xStart, (nint)(uint)xBytesConsumed);
            yStart = ref Unsafe.AddByteOffset(ref yStart, (nint)(uint)yBytesConsumed);
        }
        while (Unsafe.IsAddressLessThan(ref xStart, ref xEnd) && Unsafe.IsAddressLessThan(ref yStart, ref yEnd));

        return _value1.Length - _value2.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteUtf16Span(ref byte valueStart, out int bytesConsumed, ref char destination, out int charsWritten)
        {
            if (UnicodeUtility.IsAsciiCodePoint(valueStart))
            {
                destination = (char)valueStart;
                bytesConsumed = 1;
                charsWritten = 1;
                return;
            }

            var length = UnicodeUtility.GetUtf8SequenceLength(valueStart);
            var span = MemoryMarshal.CreateReadOnlySpan(ref valueStart, length);
            Rune.DecodeFromUtf8(span, out var rune, out bytesConsumed);

            rune.TryEncodeToUtf16(MemoryMarshal.CreateSpan(ref destination, 2), out charsWritten);
        }
    }

    [Benchmark]
    public int Compare2()
    {
        Span<char> xBuffer = stackalloc char[2];
        Span<char> yBuffer = stackalloc char[2];

        ref var xBufferStart = ref MemoryMarshal.GetReference(xBuffer);
        ref var yBufferStart = ref MemoryMarshal.GetReference(yBuffer);

        ref var xStart = ref MemoryMarshal.GetArrayDataReference(_value1);
        ref var yStart = ref MemoryMarshal.GetArrayDataReference(_value2);

        ref var xEnd = ref Unsafe.AddByteOffset(ref xStart, (nint)(uint)_value1.Length);
        ref var yEnd = ref Unsafe.AddByteOffset(ref yStart, (nint)(uint)_value2.Length);

        do
        {
            WriteUtf16Span(ref xStart, out var xBytesConsumed, xBuffer, out var charsWritten);
            var xSpan = MemoryMarshal.CreateReadOnlySpan(ref xBufferStart, charsWritten);

            WriteUtf16Span(ref yStart, out var yBytesConsumed, yBuffer, out charsWritten);
            var ySpan = MemoryMarshal.CreateReadOnlySpan(ref yBufferStart, charsWritten);

            var result = xSpan.CompareTo(ySpan, StringComparison.OrdinalIgnoreCase);

            if (result != 0)
            {
                return result;
            }

            xStart = ref Unsafe.AddByteOffset(ref xStart, (nint)(uint)xBytesConsumed);
            yStart = ref Unsafe.AddByteOffset(ref yStart, (nint)(uint)yBytesConsumed);
        }
        while (Unsafe.IsAddressLessThan(ref xStart, ref xEnd) && Unsafe.IsAddressLessThan(ref yStart, ref yEnd));

        return _value1.Length - _value2.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteUtf16Span(ref byte valueStart, out int bytesConsumed, Span<char> destination, out int charsWritten)
        {
            if (UnicodeUtility.IsAsciiCodePoint(valueStart))
            {
                MemoryMarshal.GetReference(destination) = (char)valueStart;
                bytesConsumed = 1;
                charsWritten = 1;
                return;
            }

            var length = UnicodeUtility.GetUtf8SequenceLength(valueStart);
            var span = MemoryMarshal.CreateReadOnlySpan(ref valueStart, length);
            Rune.DecodeFromUtf8(span, out var rune, out bytesConsumed);

            rune.TryEncodeToUtf16(destination, out charsWritten);
        }
    }
}
