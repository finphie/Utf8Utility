using System.Text;

namespace Utf8Utility.Benchmarks.Helpers;

/// <summary>
/// <see cref="string"/>関連のヘルパークラス。
/// </summary>
static class StringHelper
{
    /// <summary>
    /// ランダムなAscii文字列を取得します。
    /// </summary>
    /// <param name="length">文字数</param>
    /// <returns>ランダムなAscii文字列を返します。</returns>
    /// <exception cref="ArgumentOutOfRangeException">文字数に0以下が指定された場合、この例外をスローします。</exception>
    public static string GetAsciiRandomString(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        var buffer = new byte[length];

        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)Random.Shared.Next(0, 0x7F);
        }

        return Encoding.ASCII.GetString(buffer);
    }
}
