using FluentAssertions;
using Utf8Utility.Text;
using Xunit;

namespace Utf8Utility.Tests.Text;

public sealed class UnicodeUtilityTest
{
    [Theory]
    [InlineData(0x00, 1)]
    [InlineData(0x7F, 1)]
    [InlineData(0xC2, 2)]
    [InlineData(0xDF, 2)]
    [InlineData(0xE0, 3)]
    [InlineData(0xEF, 3)]
    [InlineData(0xF0, 4)]
    [InlineData(0xF4, 4)]
    public void GetUtf8SequenceLength(byte value, int length)
        => UnicodeUtility.GetUtf8SequenceLength(value).Should().Be(length);

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x7F)]
    public void IsAsciiCodePoint_Ascii文字_trueを返す(byte value)
        => UnicodeUtility.IsAsciiCodePoint(value).Should().BeTrue();

    [Theory]
    [InlineData(0xC2)]
    [InlineData(0xDF)]
    [InlineData(0xE0)]
    [InlineData(0xEF)]
    [InlineData(0xF0)]
    [InlineData(0xF4)]
    public void IsAsciiCodePoint_Ascii文字以外_falseを返す(byte value)
        => UnicodeUtility.IsAsciiCodePoint(value).Should().BeFalse();

    [Fact]
    public void IsEmptyOrWhiteSpace_空白_trueを返す()
    {
        // https://github.com/dotnet/runtime/blob/82d667c6572e85945aa3a02d7f98802db587c0d2/src/libraries/Common/tests/Tests/System/StringTests.cs#L26
        UnicodeUtility.IsEmptyOrWhiteSpace(""u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace(" "u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\n"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\r"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\t"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\u00A0"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\u000b"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\u000c"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\u0085"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\u1680"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace(" \n "u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("　\n　"u8).Should().BeTrue();
        UnicodeUtility.IsEmptyOrWhiteSpace("\u1680\u1680"u8).Should().BeTrue();
    }

    [Fact]
    public void IsEmptyOrWhiteSpace_空白以外_falseを返す()
    {
        UnicodeUtility.IsEmptyOrWhiteSpace("a"u8).Should().BeFalse();
        UnicodeUtility.IsEmptyOrWhiteSpace("α"u8).Should().BeFalse();
        UnicodeUtility.IsEmptyOrWhiteSpace("あ"u8).Should().BeFalse();
        UnicodeUtility.IsEmptyOrWhiteSpace("𩸽"u8).Should().BeFalse();
        UnicodeUtility.IsEmptyOrWhiteSpace(" a "u8).Should().BeFalse();
        UnicodeUtility.IsEmptyOrWhiteSpace("\u1680a "u8).Should().BeFalse();
        UnicodeUtility.IsEmptyOrWhiteSpace(" 𩸽"u8).Should().BeFalse();
    }
}
