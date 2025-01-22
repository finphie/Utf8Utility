using Shouldly;
using Utf8Utility.Tests.Helpers;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayIsAsciiTest
{
    const int Length = 32 * 4;

    [Fact]
    public void Ascii文字_trueを返す()
    {
        for (var i = 1; i <= Length; i++)
        {
            var ascii = StringHelper.GetAsciiRandomString(i);
            var value = new Utf8Array(ascii);

            value.IsAscii().ShouldBeTrue($"index: {i}");
        }
    }

    [Fact]
    public void 非Ascii文字_falseを返す()
    {
        for (var i = 0; i <= Length; i++)
        {
            var ascii = StringHelper.GetAsciiRandomBytes(i).ToList();
            ascii.Add(0x80);

            var value = new Utf8Array(ascii.ToArray());
            value.IsAscii().ShouldBeFalse($"index: {i}");
        }
    }

    [Fact]
    public void 空文字_falseを返す()
    {
        var value = new Utf8Array(string.Empty);
        value.IsAscii().ShouldBeFalse();
    }
}
