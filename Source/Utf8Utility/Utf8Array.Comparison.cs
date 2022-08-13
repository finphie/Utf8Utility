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
        nuint xIndex = 0;
        nuint yIndex = 0;

        Span<char> xBuffer = stackalloc char[2];
        Span<char> yBuffer = stackalloc char[2];

        while ((int)xIndex < x.ByteCount && (int)yIndex < y.ByteCount)
        {
            ref var xValueStart = ref Unsafe.AddByteOffset(ref x.DangerousGetReference(), xIndex);
            ref var yValueStart = ref Unsafe.AddByteOffset(ref y.DangerousGetReference(), yIndex);

            // Ascii文字の場合のみ、処理を最適化する。
            scoped ReadOnlySpan<char> xSpan;
            if (UnicodeUtility.IsAsciiCodePoint(xValueStart))
            {
                var charXValue = (char)xValueStart;
                scoped ref var charXValueStart = ref charXValue;

                xSpan = GetAsciiSpan(charXValueStart);
                xIndex++;
            }
            else
            {
                WriteUtf16Span(ref xValueStart, x.ByteCount - (int)xIndex, out var bytesConsumed, xBuffer, out var charsWritten);
                xSpan = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(xBuffer), charsWritten);
                xIndex += (uint)bytesConsumed;
            }

            // Ascii文字の場合のみ、処理を最適化する。
            scoped ReadOnlySpan<char> ySpan;
            if (UnicodeUtility.IsAsciiCodePoint(yValueStart))
            {
                var charYValue = (char)yValueStart;
                scoped ref var charYValueStart = ref charYValue;

                ySpan = GetAsciiSpan(charYValueStart);
                yIndex++;
            }
            else
            {
                WriteUtf16Span(ref yValueStart, y.ByteCount - (int)yIndex, out var bytesConsumed, yBuffer, out var charsWritten);
                ySpan = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(yBuffer), charsWritten);
                yIndex += (uint)bytesConsumed;
            }

            var result = xSpan.CompareTo(ySpan, comparisonType);

            if (result != 0)
            {
                return result;
            }
        }

        // 到達条件
        // 1. 比較対象の片方または両方のUTF-8配列が空。
        // 2. 途中まで文字が一致しており、片方または両方のUTF-8配列の末尾に到達。
        // したがって、バイト数の差を比較すれば良い。
        return x.ByteCount - y.ByteCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlySpan<char> GetAsciiSpan(in char valueStart)
#if NET7_0_OR_GREATER
            => new(valueStart);
#else
            => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(valueStart), 1);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteUtf16Span(ref byte valueStart, int length, out int bytesConsumed, Span<char> destination, out int charsWritten)
        {
            var span = MemoryMarshal.CreateReadOnlySpan(ref valueStart, length);
            Rune.DecodeFromUtf8(span, out var rune, out bytesConsumed);

            // UTF-16にエンコードできないことはないはず。
            // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/Text/Rune.cs#L997-L1039
            rune.TryEncodeToUtf16(destination, out charsWritten);
        }
    }
}
#endif
