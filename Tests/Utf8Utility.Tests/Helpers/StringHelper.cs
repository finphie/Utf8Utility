using System.Text;

#if !NET8_0_OR_GREATER
using CommunityToolkit.Diagnostics;
#endif

namespace Utf8Utility.Tests.Helpers;

/// <summary>
/// <see cref="string"/>関連のヘルパークラス。
/// </summary>
sealed class StringHelper
{
    const int Seed = 20221106;

    static readonly Random Random = new(Seed);

    /// <summary>
    /// ランダムなAscii文字列を取得します。
    /// </summary>
    /// <param name="length">文字数</param>
    /// <returns>ランダムなAscii文字列を返します。</returns>
    /// <exception cref="ArgumentOutOfRangeException">文字数に0未満が指定された場合、この例外をスローします。</exception>
    public static string GetAsciiRandomString(int length)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(length);
#else
        Guard.IsGreaterThanOrEqualTo(length, 0);
#endif

        if (length == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[length];

        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)Random.Next(0, 0x7F);
        }

        return Encoding.ASCII.GetString(buffer);
    }

    /// <summary>
    /// ランダムなAsciiバイト列を取得します。
    /// </summary>
    /// <param name="length">文字数</param>
    /// <returns>ランダムなAsciiバイト列を返します。</returns>
    /// <exception cref="ArgumentOutOfRangeException">バイト数に0未満が指定された場合、この例外をスローします。</exception>
    public static byte[] GetAsciiRandomBytes(int length)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegative(length);
#else
        Guard.IsGreaterThanOrEqualTo(length, 0);
#endif

        return Encoding.UTF8.GetBytes(GetAsciiRandomString(length));
    }
}
