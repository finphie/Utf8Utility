#if NET7_0_OR_GREATER
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
        if (x.IsEmpty || y.IsEmpty)
        {
            goto Count;
        }

        Span<char> xBuffer = stackalloc char[2];
        Span<char> yBuffer = stackalloc char[2];

        ref var xBufferStart = ref MemoryMarshal.GetReference(xBuffer);
        ref var yBufferStart = ref MemoryMarshal.GetReference(yBuffer);

        ref var xStart = ref x.DangerousGetReference();
        ref var yStart = ref y.DangerousGetReference();

        ref var xEnd = ref Unsafe.AddByteOffset(ref xStart, (nint)(uint)x.ByteCount);
        ref var yEnd = ref Unsafe.AddByteOffset(ref yStart, (nint)(uint)y.ByteCount);

        do
        {
            WriteUtf16Span(ref xStart, out var xBytesConsumed, xBuffer, out var charsWritten);
            var xSpan = MemoryMarshal.CreateReadOnlySpan(ref xBufferStart, charsWritten);

            WriteUtf16Span(ref yStart, out var yBytesConsumed, yBuffer, out charsWritten);
            var ySpan = MemoryMarshal.CreateReadOnlySpan(ref yBufferStart, charsWritten);

            var result = xSpan.CompareTo(ySpan, comparisonType);

            if (result != 0)
            {
                return result;
            }

            xStart = ref Unsafe.AddByteOffset(ref xStart, (nint)(uint)xBytesConsumed);
            yStart = ref Unsafe.AddByteOffset(ref yStart, (nint)(uint)yBytesConsumed);
        }
        while (Unsafe.IsAddressLessThan(ref xStart, ref xEnd) && Unsafe.IsAddressLessThan(ref yStart, ref yEnd));

    Count:

        // 到達条件
        // 1. 比較対象の片方または両方のUTF-8配列が空。
        // 2. 途中まで文字が一致しており、片方または両方のUTF-8配列の末尾に到達。
        // したがって、バイト数の差を比較すれば良い。
        return x.ByteCount - y.ByteCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteUtf16Span(ref byte valueStart, out int bytesConsumed, Span<char> destination, out int charsWritten)
        {
            // Ascii文字の場合のみ、処理を最適化する。
            if (UnicodeUtility.IsAsciiCodePoint(valueStart))
            {
                MemoryMarshal.GetReference(destination) = (char)valueStart;
                bytesConsumed = 1;
                charsWritten = 1;
                return;
            }

            // UTF-8文字を1文字だけ取得する。
            var length = UnicodeUtility.GetUtf8SequenceLength(valueStart);
            var span = MemoryMarshal.CreateReadOnlySpan(ref valueStart, length);
            Rune.DecodeFromUtf8(span, out var rune, out bytesConsumed);

            // UTF-16にエンコードできないことはないはず。
            // https://github.com/dotnet/runtime/blob/v7.0.0-rc.1.22426.10/src/libraries/System.Private.CoreLib/src/System/Text/Rune.cs#L992-L1034
            rune.TryEncodeToUtf16(destination, out charsWritten);
        }
    }
}
#endif
