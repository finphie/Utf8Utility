using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayGetCharsTest
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void 出力先のサイズが十分_Utf16配列の長さを返す(string value)
    {
        var array = new Utf8Array(value);
        var buffer = new char[value.Length];

        var charsWritten = array.GetChars(buffer);
        charsWritten.Should().Be(value.Length);
        value.Should().Be(new(buffer, 0, charsWritten));
    }

    [Fact]
    public void 出力先のサイズが不足_ArgumentException()
    {
        var array = new Utf8Array("abc");
        array.Invoking(static x =>
        {
            var buffer = new char[2];
            x.GetChars(buffer);
        })
        .Should()
        .Throw<ArgumentException>();
    }
}
