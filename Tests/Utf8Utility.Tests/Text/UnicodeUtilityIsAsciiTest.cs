using FluentAssertions;
using Utf8Utility.Tests.Helpers;
using Utf8Utility.Text;
using Xunit;

namespace Utf8Utility.Tests.Text;

public sealed class UnicodeUtilityIsAsciiTest
{
    const int Length = 32 * 4;

    [Fact]
    public void Ascii文字_trueを返す()
    {
        for (var i = 1; i <= Length; i++)
        {
            var ascii = StringHelper.GetAsciiRandomBytes(i);
            UnicodeUtility.IsAscii(ascii).Should().BeTrue($"index: {i}");
        }
    }

    [Fact]
    public void 非Ascii文字_falseを返す()
    {
        for (var i = 0; i <= Length; i++)
        {
            var ascii = StringHelper.GetAsciiRandomBytes(i).ToList();
            ascii.Add(0x80);

            UnicodeUtility.IsAscii(ascii.ToArray()).Should().BeFalse($"index: {i}");
        }
    }

    [Fact]
    public void 空文字_falseを返す()
        => UnicodeUtility.IsAscii([]).Should().BeFalse();
}
