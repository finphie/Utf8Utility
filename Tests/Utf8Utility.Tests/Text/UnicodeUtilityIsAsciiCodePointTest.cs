using Shouldly;
using Utf8Utility.Text;
using Xunit;

namespace Utf8Utility.Tests.Text;

public sealed class UnicodeUtilityIsAsciiCodePointTest
{
    [Theory]
    [InlineData(0x00)]
    [InlineData(0x7F)]
    public void Ascii文字_trueを返す(byte value)
        => UnicodeUtility.IsAsciiCodePoint(value).ShouldBeTrue();

    [Theory]
    [InlineData(0xC2)]
    [InlineData(0xDF)]
    [InlineData(0xE0)]
    [InlineData(0xEF)]
    [InlineData(0xF0)]
    [InlineData(0xF4)]
    public void 非Ascii文字_falseを返す(byte value)
        => UnicodeUtility.IsAsciiCodePoint(value).ShouldBeFalse();
}
