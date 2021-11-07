using System.Globalization;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayGetLengthTest
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
    public void Test(string value)
    {
        var array = new Utf8Array(value);
        var info = new StringInfo(value);
        array.GetLength().Should().Be(info.LengthInTextElements);
    }

    [Fact]
    public void LongTest()
    {
        const int Count = 32;
        var builder = new StringBuilder();

        for (var i = 0; i < Count; i++)
        {
            builder.Append("aαあ𩸽");
        }

        var value = builder.ToString();
        var array = new Utf8Array(value);
        var info = new StringInfo(value);

        array.GetLength().Should().Be(info.LengthInTextElements);
    }
}
