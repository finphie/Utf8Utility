#if NET6_0_OR_GREATER
using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayCompareTest
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
    public void 同じ文字列_CurrentCulture_0を返す(string value)
    {
        var x1 = new Utf8Array(value);
        var x2 = new Utf8Array(value);

        Utf8Array.Compare(x1, x2).Should().Be(0);
        x1.CompareTo(x2).Should().Be(0);
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
    [InlineData("α", "β")]
    [InlineData("α", "あ")]
    [InlineData("α", "𩸽")]
    [InlineData("あ", "い")]
    [InlineData("あ", "𩸽")]
    [InlineData("a", "ab")]
    [InlineData("ab", "aα")]
    [InlineData("!", "0")]
    [InlineData("{", "0")]
    [InlineData("0", "a")]
    [InlineData("a", "A")]
    public void 異なる文字列_CurrentCulture_0より小さい数値を返す(string value1, string value2)
    {
        var x1 = new Utf8Array(value1);
        var x2 = new Utf8Array(value2);

        Utf8Array.Compare(x1, x2).Should().BeNegative();
        x1.CompareTo(x2).Should().BeNegative();
    }
}
#endif
