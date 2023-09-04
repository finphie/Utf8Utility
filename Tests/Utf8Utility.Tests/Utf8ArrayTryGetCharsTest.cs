using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayTryGetCharsTest
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void 出力先のサイズが十分_trueを返す(string value)
    {
        var array = new Utf8Array(value);
        var buffer = new char[value.Length];

        array.TryGetChars(buffer, out var charsWritten).Should().BeTrue();
        charsWritten.Should().Be(value.Length);
        value.Should().Be(new(buffer, 0, charsWritten));
    }

    [Fact]
    public void 出力先のサイズが不足_falseを返す()
    {
        var array = new Utf8Array("abc");
        var buffer = new char[2];

        array.TryGetChars(buffer, out var charsWritten).Should().BeFalse();
        charsWritten.Should().Be(0);
    }
}
