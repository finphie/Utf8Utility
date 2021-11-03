using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayIsEmptyTest
{
    [Fact]
    public void 空文字列同士_trueを返す()
    {
        var values = new[]
        {
            Utf8Array.Empty,
            new(Array.Empty<byte>()),
            new(string.Empty),
            new(ReadOnlySpan<byte>.Empty),
            new(ReadOnlySpan<char>.Empty)
        };

        foreach (var value in values)
        {
            value.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData("a")]
    [InlineData(" ")]
    public void 空文字列以外_falseを返す(string value)
        => new Utf8Array(value).IsEmpty.Should().BeFalse();
}
