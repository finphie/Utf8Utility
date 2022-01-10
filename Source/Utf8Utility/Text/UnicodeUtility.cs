using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Utf8Utility.Text;

/// <summary>
/// Unicode関連のユーティリティクラスです。
/// </summary>
public static partial class UnicodeUtility
{
    /// <summary>
    /// 指定されたUTF-8値のバイト数を取得します。
    /// </summary>
    /// <param name="value">値</param>
    /// <returns>指定されたUTF-8値のバイト数を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetUtf8SequenceLength(byte value)
    {
        ReadOnlySpan<byte> trailingBytesForUTF8 = new byte[]
        {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
            4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        ref var table = ref MemoryMarshal.GetReference(trailingBytesForUTF8);
        return Unsafe.AddByteOffset(ref table, (nint)value);
    }

    /// <summary>
    /// 指定された値がAscii文字かどうかを判定します。
    /// </summary>
    /// <param name="value">値</param>
    /// <returns>
    /// Ascii文字の場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiCodePoint(byte value) => value <= 0x7Fu;
}
