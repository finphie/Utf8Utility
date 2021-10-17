using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayEqualsTest
{
    [Theory]
    [InlineData("abc")]
    public void 同じ文字列_trueを返す(string value)
    {
        var x1 = new Utf8Array(value);
        var x2 = new Utf8Array(value);

        x1.Equals(x2).Should().BeTrue();
    }

    [Theory]
    [InlineData("abc", "abcdef")]
    [InlineData("abc", "")]
    public void 異なる文字列_falseを返す(string value1, string value2)
    {
        var x1 = new Utf8Array(value1);
        var x2 = new Utf8Array(value2);

        x1.Equals(x2).Should().BeFalse();
    }

    [Fact]
    public void 空文字列同士_trueを返す()
    {
        var x1 = new Utf8Array(string.Empty);
        var x2 = new Utf8Array(string.Empty);

        x1.Equals(x2).Should().BeTrue();
        x1.Equals(Utf8Array.Empty).Should().BeTrue();
    }

    [Theory]
    [InlineData("abc")]
    public void Boxing_同じ文字列_trueを返す(string value)
    {
        var x1 = new Utf8Array(value);
        var x2 = (object)new Utf8Array(value);

        x1.Equals(x2).Should().BeTrue();
    }

    [Theory]
    [InlineData("abc", "abcdef")]
    [InlineData("abc", "")]
    [InlineData("abc", 1)]
    [InlineData("a", 'a')]
    public void Boxing_異なる文字列_falseを返す(string value1, object value2)
    {
        var x1 = new Utf8Array(value1);
        var x2 = value2 switch
        {
            string x => new Utf8Array(x),
            _ => value2
        };

        x1.Equals(x2).Should().BeFalse();
    }

    [Fact]
    public void Boxing_空文字列同士_trueを返す()
    {
        var x1 = new Utf8Array(string.Empty);
        var x2 = (object)new Utf8Array(string.Empty);

        x1.Equals(x2).Should().BeTrue();
        x1.Equals((object)Utf8Array.Empty).Should().BeTrue();
    }
}