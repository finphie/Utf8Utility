using FluentAssertions;
using Utf8Utility.Text;
using Xunit;

namespace Utf8Utility.Tests.Text;

public sealed class UnicodeUtilityGetUtf8SequenceLengthTest
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
    public void Utf8文字の先頭バイト_文字のバイト数を返す(byte value, int length)
        => UnicodeUtility.GetUtf8SequenceLength(value).Should().Be(length);
}
