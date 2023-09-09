using System.Globalization;
using System.Text;
using FluentAssertions;
using Utf8Utility.Text;
using Xunit;

namespace Utf8Utility.Tests.Text;

public sealed class UnicodeUtilityGetLengthTest
{
    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("abc")]
    [InlineData("α")]
    [InlineData("αβγ")]
    [InlineData("あ")]
    [InlineData("あいう")]
    [InlineData("𩸽")]
    [InlineData("𩸽😀🖳")]
    [InlineData("aα")]
    [InlineData("aあ")]
    [InlineData("a𩸽")]
    [InlineData("0123456789012345678901234567890")]
    [InlineData("01234567890123456789012345678901")]
    [InlineData("012345678901234567890123456789012")]
    [InlineData("012345678901234567890123456789010123456789012345678901234567890")]
    [InlineData("0123456789012345678901234567890101234567890123456789012345678901")]
    [InlineData("01234567890123456789012345678901012345678901234567890123456789012")]
    [InlineData("𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳𩸽😀🖳")]
    public void 文字列_長さを返す(string value)
    {
        var utf8 = Encoding.UTF8.GetBytes(value);
        var info = new StringInfo(value);

        UnicodeUtility.GetLength(utf8).Should().Be(info.LengthInTextElements);
    }

    [Theory]
    [InlineData((255 * 32) - 1)]
    [InlineData(255 * 32)]
    [InlineData((255 * 32) + 1)]
    [InlineData(((255 + 1) * 32) - 1)]
    [InlineData((255 + 1) * 32)]
    [InlineData(((255 + 1) * 32) + 1)]
    [InlineData(10000)]
    public void 長い文字列_長さを返す(int length)
    {
        var value = new string('a', length);
        var utf8 = Encoding.UTF8.GetBytes(value);
        var info = new StringInfo(value);

        UnicodeUtility.GetLength(utf8).Should().Be(info.LengthInTextElements);
    }
}
