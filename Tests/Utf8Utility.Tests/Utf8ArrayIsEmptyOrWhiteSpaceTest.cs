﻿#if NET7_0_OR_GREATER
using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayIsEmptyOrWhiteSpaceTest
{
    // https://github.com/dotnet/runtime/blob/82d667c6572e85945aa3a02d7f98802db587c0d2/src/libraries/Common/tests/Tests/System/StringTests.cs#L26
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
        => new Utf8Array(value).IsEmptyOrWhiteSpace().Should().BeTrue();

    [Theory]
    [InlineData("a")]
    [InlineData("α")]
    [InlineData("あ")]
    [InlineData("𩸽")]
    [InlineData(" a ")]
    [InlineData("\u1680a ")]
    [InlineData(" 𩸽")]
    public void 空またはスペース_falseを返す(string value)
        => new Utf8Array(value).IsEmptyOrWhiteSpace().Should().BeFalse();
}
#endif
