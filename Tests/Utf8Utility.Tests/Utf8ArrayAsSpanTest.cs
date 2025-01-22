using System.Text;
using Shouldly;
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

        span.SequenceEqual(array).ShouldBeTrue();
    }

    [Theory]
    [InlineData("abcdef")]
    public void 初期インデックス設定(string value)
    {
        var utf8Value = Encoding.UTF8.GetBytes(value);

        for (var i = 0; i < value.Length; i++)
        {
            var span = new Utf8Array(utf8Value).AsSpan(i);
            var array = utf8Value.AsSpan(i);

            span.SequenceEqual(array).ShouldBeTrue();
        }
    }
}
