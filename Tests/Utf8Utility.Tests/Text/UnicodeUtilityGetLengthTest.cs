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
    public void 文字列_長さを返す(string value)
    {
        var utf8 = Encoding.UTF8.GetBytes(value);
        var info = new StringInfo(value);

        UnicodeUtility.GetLength(utf8).Should().Be(info.LengthInTextElements);
    }
}
