#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Utf8Utility.Text;

namespace Utf8Utility;

/// <content>
/// <see cref="Utf8Array"/>構造体の比較関連処理です。
/// </content>
partial struct Utf8Array
{
    /// <summary>
    /// 文字コード順の比較を行います。
    /// </summary>
    /// <param name="x">基準となるUTF-8配列</param>
    /// <param name="y">比較対象のUTF-8配列</param>
    /// <returns>
    /// <paramref name="x"/>が<paramref name="y"/>より小さいか等しいか大きいかを判断して、
    /// 負の整数か0または正の整数を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareOrdinal(Utf8Array x, Utf8Array y) => x.AsSpan().SequenceCompareTo(y.AsSpan());

    /// <summary>
    /// 比較を行います。
    /// </summary>
    /// <param name="x">基準となるUTF-8配列</param>
    /// <param name="y">比較対象のUTF-8配列</param>
    /// <returns>
    /// <paramref name="x"/>が<paramref name="y"/>より小さいか等しいか大きいかを判断して、
    /// 負の整数か0または正の整数を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(Utf8Array x, Utf8Array y) => Compare(x, y, StringComparison.CurrentCulture);

    /// <summary>
    /// 比較を行います。
    /// </summary>
    /// <param name="x">基準となるUTF-8配列</param>
    /// <param name="y">比較対象のUTF-8配列</param>
    /// <param name="comparisonType">比較規則</param>
    /// <returns>
    /// <paramref name="x"/>が<paramref name="y"/>より小さいか等しいか大きいかを判断して、
    /// 負の整数か0または正の整数を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(Utf8Array x, Utf8Array y, StringComparison comparisonType)
    {
        nuint index = 0;
        nuint length = (uint)Math.Min(x.ByteCount, y.ByteCount);

        while (index < length)
        {
            ref var xStart = ref x.DangerousGetReference();
            ref var yStart = ref y.DangerousGetReference();

            ref var xValue = ref Unsafe.AddByteOffset(ref xStart, (nint)index);
            ref var yValue = ref Unsafe.AddByteOffset(ref yStart, (nint)index);

            var xByteCount = UnicodeUtility.GetUtf8SequenceLength(xValue);
            var yByteCount = UnicodeUtility.GetUtf8SequenceLength(yValue);
            var diffUtf8SequenceLength = xByteCount - yByteCount;

            // 最初の要素が異なるバイト数の文字だった場合、バイト数が短い順にする。
            if (diffUtf8SequenceLength != 0)
            {
                return diffUtf8SequenceLength;
            }

            // Ascii文字の場合のみ、処理を最適化する。
            // xByteCount == yByteCountなのでyByteCountでの条件分岐は不要。
            if (xByteCount == 1)
            {
                // Ascii文字なのでbyte型からchar型への変換を行っても問題ない。
                var xCharValue = (char)xValue;
                var yCharValue = (char)yValue;

                var xSpanValue = MemoryMarshal.CreateReadOnlySpan(ref xCharValue, 1);
                var ySpanValue = MemoryMarshal.CreateReadOnlySpan(ref yCharValue, 1);
                var result = xSpanValue.CompareTo(ySpanValue, comparisonType);

                if (result != 0)
                {
                    return result;
                }

                // Ascii文字なので1を加算
                index++;
                continue;
            }

            var xSpan = MemoryMarshal.CreateReadOnlySpan(ref xValue, x.ByteCount - (int)index);
            var ySpan = MemoryMarshal.CreateReadOnlySpan(ref yValue, y.ByteCount - (int)index);

            Rune.DecodeFromUtf8(xSpan, out var xRune, out _);
            Rune.DecodeFromUtf8(ySpan, out var yRune, out _);

            Unsafe.SkipInit(out long xBuffer);
            Unsafe.SkipInit(out long yBuffer);

            var xBufferSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<long, char>(ref xBuffer), 2);
            var yBufferSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<long, char>(ref yBuffer), 2);

            var xLength = xRune.EncodeToUtf16(xBufferSpan);
            var yLength = yRune.EncodeToUtf16(yBufferSpan);

            var result2 = ((ReadOnlySpan<char>)xBufferSpan.Slice(0, xLength)).CompareTo(yBufferSpan.Slice(0, yLength), comparisonType);

            if (result2 != 0)
            {
                return result2;
            }

            index += (uint)xByteCount;
        }

        return x.ByteCount - y.ByteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare2(Utf8Array x, Utf8Array y, StringComparison comparisonType)
    {
        nuint index = 0;
        nuint length = (uint)Math.Min(x.ByteCount, y.ByteCount);

        Unsafe.SkipInit(out long xBuffer);
        Unsafe.SkipInit(out long yBuffer);

        var xBufferSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<long, char>(ref xBuffer), 2);
        var yBufferSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<long, char>(ref yBuffer), 2);

        while (index < length)
        {
            ref var xStart = ref x.DangerousGetReference();
            ref var yStart = ref y.DangerousGetReference();

            ref var xValue = ref Unsafe.AddByteOffset(ref xStart, (nint)index);
            ref var yValue = ref Unsafe.AddByteOffset(ref yStart, (nint)index);

            var xByteCount = UnicodeUtility.GetUtf8SequenceLength(xValue);
            var yByteCount = UnicodeUtility.GetUtf8SequenceLength(yValue);
            var diffUtf8SequenceLength = xByteCount - yByteCount;

            // 最初の要素が異なるバイト数の文字だった場合、バイト数が短い順にする。
            if (diffUtf8SequenceLength != 0)
            {
                return diffUtf8SequenceLength;
            }

            // Ascii文字の場合のみ、処理を最適化する。
            // xByteCount == yByteCountなのでyByteCountでの条件分岐は不要。
            if (xByteCount == 1)
            {
                // Ascii文字なのでbyte型からchar型への変換を行っても問題ない。
                var xCharValue = (char)xValue;
                var yCharValue = (char)yValue;

                var xSpanValue = MemoryMarshal.CreateReadOnlySpan(ref xCharValue, 1);
                var ySpanValue = MemoryMarshal.CreateReadOnlySpan(ref yCharValue, 1);
                var result = xSpanValue.CompareTo(ySpanValue, comparisonType);

                if (result != 0)
                {
                    return result;
                }

                // Ascii文字なので1を加算
                index++;
                continue;
            }

            var xSpan = MemoryMarshal.CreateReadOnlySpan(ref xValue, x.ByteCount - (int)index);
            var ySpan = MemoryMarshal.CreateReadOnlySpan(ref yValue, y.ByteCount - (int)index);

            Rune.DecodeFromUtf8(xSpan, out var xRune, out _);
            Rune.DecodeFromUtf8(ySpan, out var yRune, out _);

            var xLength = xRune.EncodeToUtf16(xBufferSpan);
            var yLength = yRune.EncodeToUtf16(yBufferSpan);

            var result2 = ((ReadOnlySpan<char>)xBufferSpan.Slice(0, xLength)).CompareTo(yBufferSpan.Slice(0, yLength), comparisonType);

            if (result2 != 0)
            {
                return result2;
            }

            index += (uint)xByteCount;
        }

        return x.ByteCount - y.ByteCount;
    }
}
#endif
