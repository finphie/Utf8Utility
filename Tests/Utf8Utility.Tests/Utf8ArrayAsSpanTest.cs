using System.Text;
using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayAsSpanTest
{
    [Theory]
    [InlineData("abc")]
    public void 初期インデックス0(string value)
    {
        var span = new Utf8Array(value).AsSpan();
        var array = Encoding.UTF8.GetBytes(value);

        span.SequenceEqual(array).Should().BeTrue();
    }

    [Theory]
    [InlineData("abcdef")]
    public void 初期インデックス3(string value)
    {
        var span = new Utf8Array(value).AsSpan(3);
        var array = Encoding.UTF8.GetBytes(value).AsSpan(3);

        span.SequenceEqual(array).Should().BeTrue();
    }
}
