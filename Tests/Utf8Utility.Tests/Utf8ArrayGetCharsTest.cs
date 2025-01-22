using Shouldly;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayGetCharsTest
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void 出力先のサイズが十分_Utf16配列の長さを返す(string value)
    {
        var array = new Utf8Array(value);
        var buffer = new char[value.Length];

        var charsWritten = array.GetChars(buffer);
        charsWritten.ShouldBe(value.Length);
        value.ShouldBe(new(buffer, 0, charsWritten));
    }

    [Fact]
    public void 出力先のサイズが不足_ArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
        {
            var array = new Utf8Array("abc");
            var buffer = new char[2];
            array.GetChars(buffer);
        });
    }
}
