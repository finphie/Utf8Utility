#if NET7_0_OR_GREATER
using System.Text;
using Shouldly;
using Utf8Utility.Text;
using Xunit;

namespace Utf8Utility.Tests.Text;

public sealed class UnicodeUtilityIsEmptyOrWhiteSpaceTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("　")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\t")]
    [InlineData("\u00A0")]
    [InlineData("\u000b")]
    [InlineData("\u000c")]
    [InlineData("\u0085")]
    [InlineData("\u1680")]
    [InlineData(" \n ")]
    [InlineData("　\n　")]
    [InlineData("\u1680\u1680")]
    public void 空またはスペース_trueを返す(string value)
        => UnicodeUtility.IsEmptyOrWhiteSpace(Encoding.UTF8.GetBytes(value)).ShouldBeTrue();

    [Theory]
    [InlineData("a")]
    [InlineData("α")]
    [InlineData("あ")]
    [InlineData("𩸽")]
    [InlineData(" a ")]
    [InlineData("\u1680a ")]
    [InlineData(" 𩸽")]
    public void 空またはスペース_falseを返す(string value)
        => UnicodeUtility.IsEmptyOrWhiteSpace(Encoding.UTF8.GetBytes(value)).ShouldBeFalse();
}
#endif
