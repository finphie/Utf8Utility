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
            ref var xValueStart = ref Unsafe.AddByteOffset(ref x.DangerousGetReference(), index);
            ref var yValueStart = ref Unsafe.AddByteOffset(ref y.DangerousGetReference(), index);

            var xFirstPointByteCount = UnicodeUtility.GetUtf8SequenceLength(xValueStart);
            var yFirstPointByteCount = UnicodeUtility.GetUtf8SequenceLength(yValueStart);
            var diffFirstPointByteCount = xFirstPointByteCount - yFirstPointByteCount;

            // 最初の要素が異なるバイト数の文字だった場合、バイト数が短い順にする。
            if (diffFirstPointByteCount != 0)
            {
                return diffFirstPointByteCount;
            }

            // Ascii文字の場合のみ、処理を最適化する。
            // xByteCount == yByteCountなのでyByteCountでの条件分岐は不要。
            if (xFirstPointByteCount == 1)
            {
                // Ascii文字なのでbyte型からchar型への変換を行っても問題ない。
                var xAsciiCode = (char)xValueStart;
                var yAsciiCode = (char)yValueStart;

                // Ascii文字は1バイト。
                var xAsciiSpan = MemoryMarshal.CreateReadOnlySpan(ref xAsciiCode, 1);
                var yAsciiSpan = MemoryMarshal.CreateReadOnlySpan(ref yAsciiCode, 1);

                var asciiResult = xAsciiSpan.CompareTo(yAsciiSpan, comparisonType);

                if (asciiResult != 0)
                {
                    return asciiResult;
                }

                // Ascii文字なので1を加算する。
                index++;
                continue;
            }

            var xSpan = MemoryMarshal.CreateReadOnlySpan(ref xValueStart, x.ByteCount - (int)index);
            var ySpan = MemoryMarshal.CreateReadOnlySpan(ref yValueStart, y.ByteCount - (int)index);

            Rune.DecodeFromUtf8(xSpan, out var xRune, out _);
            Rune.DecodeFromUtf8(ySpan, out var yRune, out _);

            // 非Ascii文字は、char1つまたは2つで表現できる。
            // したがって、バッファはchar2つ分（4バイト）以上必要なのでnintで定義する。
            Unsafe.SkipInit(out nint xBuffer);
            Unsafe.SkipInit(out nint yBuffer);
            var xBufferSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<nint, char>(ref xBuffer), 2);
            var yBufferSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<nint, char>(ref yBuffer), 2);

            // UTF-16にエンコードできないことはないはず。
            // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/Text/Rune.cs#L997-L1039
            xRune.TryEncodeToUtf16(xBufferSpan, out var xCharsWritten);
            yRune.TryEncodeToUtf16(yBufferSpan, out var yCharsWritten);

            var xNonAsciiSpan = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(xBufferSpan), xCharsWritten);
            var yNonAsciiSpan = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(yBufferSpan), yCharsWritten);

            var nonAsciiResult = xNonAsciiSpan.CompareTo(yNonAsciiSpan, comparisonType);

            if (nonAsciiResult != 0)
            {
                return nonAsciiResult;
            }

            // 非Ascii文字なので2～4を加算
            index += (uint)xFirstPointByteCount;
        }

        return x.ByteCount - y.ByteCount;
    }
}
#endif
