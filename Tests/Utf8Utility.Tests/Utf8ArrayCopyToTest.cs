using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayCopyToTest
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void コピー先のサイズが十分_正常終了(string value)
    {
        var array = new Utf8Array(value);
        var buffer = new byte[array.ByteCount];

        array.CopyTo(buffer);
        array.DangerousAsByteArray().Should().Equal(buffer);
    }

    [Fact]
    public void コピー先のサイズが不足_ArgumentException()
    {
        var array = new Utf8Array("abc");
        array.Invoking(static x =>
        {
            var buffer = new byte[2];
            x.CopyTo(buffer);
        })
            .Should()
            .Throw<ArgumentException>();
    }
}
