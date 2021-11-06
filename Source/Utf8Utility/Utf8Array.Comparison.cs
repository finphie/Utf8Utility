#if NET6_0_OR_GREATER
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
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
        var index = 0;
        var length = Math.Min(x.ByteCount, y.ByteCount);

        while (index < length)
        {
            ref var xStart = ref x.DangerousGetReference();
            ref var yStart = ref y.DangerousGetReference();

            var xValue = Unsafe.AddByteOffset(ref xStart, (nint)(uint)index);
            var yValue = Unsafe.AddByteOffset(ref yStart, (nint)(uint)index);

            var xByteCount = UnicodeUtility.GetUtf8SequenceLength(xValue);
            var yByteCount = UnicodeUtility.GetUtf8SequenceLength(yValue);
            var diffUtf8SequenceLength = xByteCount - yByteCount;

            // 最初の要素が異なるバイト数の文字だった場合、バイト数が短い順にする。
            if (diffUtf8SequenceLength != 0)
            {
                return diffUtf8SequenceLength;
            }

            // 非Ascii文字の場合、UTF-16文字列に変換して比較する。
            // xByteCount == yByteCountなのでyByteCountでの条件分岐は不要。
            if (xByteCount != 1)
            {
                return Utf16Compare(x.AsSpan(index), y.AsSpan(index), comparisonType);
            }

            // Ascii文字なのでbyte型からchar型への変換を行っても問題ない。
            var xCharValue = (char)xStart;
            var yCharValue = (char)yStart;

            var xSpanValue = MemoryMarshal.CreateReadOnlySpan(ref xCharValue, 1);
            var ySpanValue = MemoryMarshal.CreateReadOnlySpan(ref yCharValue, 1);
            var result = xSpanValue.CompareTo(ySpanValue, comparisonType);

            if (result != 0)
            {
                return result;
            }

            // Ascii文字なので1を加算
            index++;
        }

        return x.ByteCount - y.ByteCount;

        static int Utf16Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y, StringComparison comparisonType)
        {
            const int StackallocThreshold = 256;

            char[]? xPool = null;
            var xCount = Encoding.UTF8.GetCharCount(x);
            Span<char> xBuffer = xCount <= StackallocThreshold
                ? stackalloc char[xCount]
                : (xPool = ArrayPool<char>.Shared.Rent(xCount));

            char[]? yPool = null;
            var yCount = Encoding.UTF8.GetCharCount(y);
            Span<char> yBuffer = yCount <= StackallocThreshold
                ? stackalloc char[yCount]
                : (yPool = ArrayPool<char>.Shared.Rent(yCount));

            var result = ToUtf16(x, xBuffer) && ToUtf16(y, yBuffer)
                ? ((ReadOnlySpan<char>)xBuffer).CompareTo(yBuffer, comparisonType)
                : x.SequenceCompareTo(y);

            if (xPool is not null)
            {
                ArrayPool<char>.Shared.Return(xPool);
            }

            if (yPool is not null)
            {
                ArrayPool<char>.Shared.Return(yPool);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool ToUtf16(ReadOnlySpan<byte> source, Span<char> destination)
            => Utf8.ToUtf16(source, destination, out _, out _) == OperationStatus.Done;
    }
}
#endif
