using Shouldly;
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

        array.TryCopyTo(buffer).ShouldBeTrue();
        array.DangerousAsByteArray().ShouldBe(buffer);
    }

    [Fact]
    public void コピー先のサイズが不足_falseを返す()
    {
        var array = new Utf8Array("abc");
        var buffer = new byte[array.ByteCount - 1];

        array.TryCopyTo(buffer).ShouldBeFalse();
        buffer.ShouldAllBe(static x => x == 0);
    }
}
