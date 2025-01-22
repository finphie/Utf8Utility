using Shouldly;
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
        array.DangerousAsByteArray().ShouldBe(buffer);
    }

    [Fact]
    public void コピー先のサイズが不足_ArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
        {
            var array = new Utf8Array("abc");
            var buffer = new byte[2];
            array.CopyTo(buffer);
        });
    }
}
