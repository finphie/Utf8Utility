#if NET6_0_OR_GREATER
using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayCompareToTest
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
    public void 同じ文字列_0を返す(string value)
    {
        var x1 = new Utf8Array(value);
        var x2 = new Utf8Array(value);

        x1.CompareTo(x2).Should().Be(0);
    }
}
#endif
