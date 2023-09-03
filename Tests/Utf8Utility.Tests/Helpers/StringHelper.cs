using System.Text;

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
    /// <exception cref="ArgumentOutOfRangeException">文字数に0以下が指定された場合、この例外をスローします。</exception>
    public static string GetAsciiRandomString(int length)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
#else
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }
#endif

        var buffer = new byte[length];

        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)Random.Next(0, 0x7F);
        }

        return Encoding.ASCII.GetString(buffer);
    }
}
