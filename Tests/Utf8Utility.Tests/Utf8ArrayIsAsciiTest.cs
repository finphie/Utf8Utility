using FluentAssertions;
using Utf8Utility.Tests.Helpers;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayIsAsciiTest
{
    [Theory]
    [InlineData(10)]
    [InlineData(127)]
    public void Ascii文字_trueを返す(int length)
    {
        var value = new Utf8Array(StringHelper.GetAsciiRandomString(length));
        value.IsAscii().Should().BeTrue();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(127)]
    public void 非Ascii文字_falseを返す(int length)
    {
        var ascii = StringHelper.GetAsciiRandomString(length - 2);

        for (var i = 0; i <= ascii.Length; i++)
        {
            var s = ascii.Insert(i, "α");
            var value = new Utf8Array(s);
            value.IsAscii().Should().BeFalse($"index: {i}");
        }
    }
}
