using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

#if NET7_0_OR_GREATER
using System.Numerics;
using System.Text;
#endif

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
        return Unsafe.Add(ref table, (nint)value);
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

    /// <summary>
    /// UTF-8文字数を取得します。
    /// </summary>
    /// <param name="value">UTF-8文字列</param>
    /// <returns>UTF-8文字数を返します。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetLength(ReadOnlySpan<byte> value)
    {
        var count = 0;
        nuint index = 0;

#if NET7_0_OR_GREATER
        const ulong Mask = 0x8080808080808080 >> 7;
        var length = value.Length - sizeof(ulong);

        // 8バイト単位でカウントする。
        while ((int)index <= length)
        {
            var number = Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref value.DangerousGetReference(), index));

            var x = ((number >> 6) | (~number >> 7)) & Mask;
            count += BitOperations.PopCount(x);
            index += sizeof(ulong);
        }
#endif

        // 1バイト単位でカウントする。
        while ((int)index < value.Length)
        {
            var number = Unsafe.AddByteOffset(ref value.DangerousGetReference(), index);

            if ((number & 0xC0) != 0x80)
            {
                count++;
            }

            index++;
        }

        return count;
    }

#if NET7_0_OR_GREATER
    /// <summary>
    /// UTF-8文字列が空または空白かどうかを判定します。
    /// </summary>
    /// <param name="value">UTF-8文字列</param>
    /// <returns>
    /// UTF-8配列が空または空白の場合は<see langword="true"/>、
    /// それ以外は<see langword="false"/>を返します。
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmptyOrWhiteSpace(ReadOnlySpan<byte> value)
    {
        // Utf8Spanを参考に実装
        // https://github.com/dotnet/runtimelab/blob/84564a0e033114a1b2316c7bfb9953e4e3255cd3/src/libraries/System.Private.CoreLib/src/System/Text/Utf8Span.cs#L124
        // https://github.com/dotnet/runtimelab/blob/84564a0e033114a1b2316c7bfb9953e4e3255cd3/src/libraries/System.Private.CoreLib/src/System/Text/Unicode/Utf8Utility.WhiteSpace.CoreLib.cs#L11-L68
        nuint index = 0;
        var length = value.Length;

        while ((int)index < length)
        {
            ref var valueStart = ref Unsafe.AddByteOffset(ref value.DangerousGetReference(), index);

            // 文字コードが[0x21..0x7F]の範囲にあるか。
            if ((sbyte)valueStart > (sbyte)' ')
            {
                break;
            }

            // Ascii文字の場合のみ、処理を最適化する。
            // 空白確認には、{Rune|char}.IsWhiteSpaceを利用できる。
            // Rune.DecodeFromUtf8は引数チェックなどがあるので遅く、
            // 回避するためにUnsafe.Asでvalueを直接書き換える必要があるが、Ascii文字限定。
            // charにキャストして比較する方法もAscii文字限定。
            // したがって、最適化を行う場合はAscii文字かどうかでの分岐は必須。
            if (IsAsciiCodePoint(valueStart))
            {
                // 直前の処理でAscii文字であることは確定しているため、
                // {Rune|char}.IsWhiteSpaceを使用せず、自前実装で比較を行う。
                // 上記メソッドではAscii文字かどうかで判定が入ってしまうため。
                // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/Text/Rune.cs#L1350-L1366
                // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/Char.cs#L274-L287
                if (AsciiUtility.IsWhiteSpace(valueStart))
                {
                    index++;
                    continue;
                }
            }
            else
            {
                var span = MemoryMarshal.CreateReadOnlySpan(ref valueStart, length - (int)index);
                Rune.DecodeFromUtf8(span, out var rune, out var bytesConsumed);

                if (Rune.IsWhiteSpace(rune))
                {
                    index += (uint)bytesConsumed;
                    continue;
                }
            }

            // ここに到達した場合、空白以外の文字のはず。
            break;
        }

        return (int)index == length;
    }
#endif
}
