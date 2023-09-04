using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayTryCopyToTest
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void コピー先のサイズが十分_trueを返す(string value)
    {
        var array = new Utf8Array(value);
        var buffer = new byte[array.ByteCount];

        array.TryCopyTo(buffer).Should().BeTrue();
        array.DangerousAsByteArray().Should().Equal(buffer);
    }

    [Fact]
    public void コピー先のサイズが不足_falseを返す()
    {
        var array = new Utf8Array("abc");
        var buffer = new byte[array.ByteCount - 1];

        array.TryCopyTo(buffer).Should().BeFalse();
        buffer.Should().OnlyContain(static x => x == 0);
    }
}
