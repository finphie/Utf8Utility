using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayDictionaryClearTest
{
    [Fact]
    public void 要素0の状態_Countが0になる()
    {
        var utf8Dict = new Utf8ArrayDictionary<int>();
        utf8Dict.Clear();
        utf8Dict.Count.Should().Be(0);
        utf8Dict.Clear();
        utf8Dict.Count.Should().Be(0);
    }

    [Fact]
    public void 要素1の状態_Countが0になる()
    {
        var utf8Dict = new Utf8ArrayDictionary<int>();
        var utf8Key = new Utf8Array("abc");
        utf8Dict.TryAdd(utf8Key, 1);
        utf8Dict.Clear();

        utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeFalse();
        utf8DictValue.Should().Be(default);
        utf8Dict.Count.Should().Be(0);
    }
}
