#if NET8_0_OR_GREATER
using Shouldly;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayTryFormatTest
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void コピー先のサイズが十分_trueを返す(string value)
    {
        var array = new Utf8Array(value);
        var buffer = new char[value.Length];

        array.TryFormat(buffer, out var charsWritten, provider: null).ShouldBeTrue();
        charsWritten.ShouldBe(value.Length);
        value.ShouldBe(new(buffer, 0, charsWritten));
    }

    [Fact]
    public void コピー先のサイズが不足_falseを返す()
    {
        var array = new Utf8Array("abc");
        var buffer = new char[2];

        array.TryFormat(buffer, out var charsWritten, provider: null).ShouldBeFalse();
        charsWritten.ShouldBe(0);
    }

#if NET8_0_OR_GREATER
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void Utf8_コピー先のサイズが十分_trueを返す(string value)
    {
        var array = new Utf8Array(value);
        var buffer = new byte[value.Length];

        array.TryFormat(buffer, out var bytesWritten, provider: null).ShouldBeTrue();
        bytesWritten.ShouldBe(array.ByteCount);
        array.DangerousAsByteArray().ShouldBe(buffer);
    }

    [Fact]
    public void Utf8_コピー先のサイズが不足_falseを返す()
    {
        var array = new Utf8Array("abc");
        var buffer = new byte[2];

        array.TryFormat(buffer, out var bytesWritten, provider: null).ShouldBeFalse();
        bytesWritten.ShouldBe(0);
    }
#endif
}
#endif
