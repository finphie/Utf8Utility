#if NET7_0_OR_GREATER
using System.Text;
using Shouldly;
using Utf8Utility.Text;
using Xunit;

namespace Utf8Utility.Tests.Text;

public sealed class UnicodeUtilityCompareTest
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
    [InlineData("👩🏽‍🚒")]
    [InlineData("𩸽😀🖳")]
    [InlineData("aα")]
    [InlineData("aあ")]
    [InlineData("a𩸽")]
    public void 同じ文字列_0を返す(string value)
    {
        var x1 = Encoding.UTF8.GetBytes(value);
        var x2 = Encoding.UTF8.GetBytes(value);

        UnicodeUtility.Compare(x1, x2).ShouldBe(0);
        UnicodeUtility.Compare(x1, x2, StringComparison.InvariantCulture).ShouldBe(0);
    }

    [Theory]
    [InlineData("", "a")]
    [InlineData("", "α")]
    [InlineData("", "あ")]
    [InlineData("", "𩸽")]
    [InlineData("a", "b")]
    [InlineData("a", "α")]
    [InlineData("a", "あ")]
    [InlineData("a", "𩸽")]
    [InlineData("👩🏽‍🚒", "a")]
    [InlineData("α", "β")]
    [InlineData("α", "あ")]
    [InlineData("α", "𩸽")]
    [InlineData("あ", "い")]
    [InlineData("あ", "𩸽")]
    [InlineData("a", "ab")]
    [InlineData("ab", "aα")]
    [InlineData("abc", "abd")]
    [InlineData("あいう", "あいえ")]
    [InlineData("!", "0")]
    [InlineData("{", "0")]
    [InlineData("0", "a")]
    [InlineData("a", "A")]
    public void 異なる文字列_0より小さい数値を返す(string value1, string value2)
    {
        var x1 = Encoding.UTF8.GetBytes(value1);
        var x2 = Encoding.UTF8.GetBytes(value2);

        UnicodeUtility.Compare(x1, x2, StringComparison.InvariantCulture).ShouldBeNegative();
    }
}
#endif
