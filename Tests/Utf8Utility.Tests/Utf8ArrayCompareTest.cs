#if NET8_0_OR_GREATER
using Shouldly;
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
    [InlineData("👩🏽‍🚒")]
    [InlineData("𩸽😀🖳")]
    [InlineData("aα")]
    [InlineData("aあ")]
    [InlineData("a𩸽")]
    public void 同じ文字列_0を返す(string value)
    {
        var x1 = new Utf8Array(value);
        var x2 = new Utf8Array(value);

        Utf8Array.Compare(x1, x2, StringComparison.CurrentCulture).ShouldBe(0);
        Utf8Array.Compare(x1, x2, StringComparison.InvariantCulture).ShouldBe(0);
        x1.CompareTo(x2).ShouldBe(0);
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
        var x1 = new Utf8Array(value1);
        var x2 = new Utf8Array(value2);

        Utf8Array.Compare(x1, x2, StringComparison.InvariantCulture).ShouldBeNegative();
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
    [InlineData("z", "{")]
    [InlineData("0", "a")]
    [InlineData("A", "a")]
    public void CompareOrdinal(string value1, string value2)
    {
        var x1 = new Utf8Array(value1);
        var x2 = new Utf8Array(value2);

        Utf8Array.CompareOrdinal(x1, x2).ShouldBeNegative();
    }
}
#endif
