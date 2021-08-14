using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8StringDictionaryTryAddTest
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void キーを追加_Countが1になる(string key)
    {
        var utf8Dict = new Utf8StringDictionary<int>();
        var utf8Key = new Utf8String(key);

        utf8Dict.Count.Should().Be(0);
        utf8Dict.TryAdd(utf8Key, 1).Should().BeTrue();
        utf8Dict.Count.Should().Be(1);
    }

    [Fact]
    public void 同じキーを2回追加_Countが1になる()
    {
        var utf8Dict = new Utf8StringDictionary<int>();
        var utf8Key = new Utf8String("abc");

        utf8Dict.Count.Should().Be(0);
        utf8Dict.TryAdd(utf8Key, 1).Should().BeTrue();
        utf8Dict.Count.Should().Be(1);
        utf8Dict.TryAdd(utf8Key, 2).Should().BeFalse();
        utf8Dict.Count.Should().Be(1);
    }

    [Fact]
    public void 異なるキーを2回追加_Countが2になる()
    {
        var utf8Dict = new Utf8StringDictionary<int>();

        utf8Dict.Count.Should().Be(0);
        utf8Dict.TryAdd(new Utf8String("abc"), 1).Should().BeTrue();
        utf8Dict.Count.Should().Be(1);
        utf8Dict.TryAdd(new Utf8String("def"), 1).Should().BeTrue();
        utf8Dict.Count.Should().Be(2);
    }
}