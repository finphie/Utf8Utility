using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayDictionaryTryGetValueTest
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void 存在するキー_値型アイテムを取得_trueを返す(string key)
    {
        var utf8Dict = new Utf8ArrayDictionary<int>();
        var utf8Key = new Utf8Array(key);

        utf8Dict.TryAdd(utf8Key, 1).Should().BeTrue();

        utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeTrue();
        utf8DictValue.Should().Be(1);

        utf8Dict.TryGetValue(utf8Key.AsSpan(), out utf8DictValue).Should().BeTrue();
        utf8DictValue.Should().Be(1);

        utf8Dict.TryGetValue(key.AsSpan(), out utf8DictValue).Should().BeTrue();
        utf8DictValue.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void 存在するキー_参照型アイテムを取得_trueを返す(string key)
    {
        var utf8Dict = new Utf8ArrayDictionary<Test>();
        var utf8Key = new Utf8Array(key);
        var value = new Test(1, "a");

        utf8Dict.TryAdd(utf8Key, value).Should().BeTrue();

        utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeTrue();
        utf8DictValue!.A.Should().Be(value.A);
        utf8DictValue.B.Should().Be(value.B);

        utf8Dict.TryGetValue(utf8Key.AsSpan(), out utf8DictValue).Should().BeTrue();
        utf8DictValue!.A.Should().Be(value.A);
        utf8DictValue.B.Should().Be(value.B);

        utf8Dict.TryGetValue(key.AsSpan(), out utf8DictValue).Should().BeTrue();
        utf8DictValue!.A.Should().Be(value.A);
        utf8DictValue.B.Should().Be(value.B);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void 存在するキー_最初に追加された値型アイテムを取得_trueを返す(string key)
    {
        var utf8Dict = new Utf8ArrayDictionary<int>();
        var utf8Key = new Utf8Array(key);

        utf8Dict.TryAdd(utf8Key, 1).Should().BeTrue();

        utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeTrue();
        utf8DictValue.Should().Be(1);

        utf8Dict.TryAdd(utf8Key, 2).Should().BeFalse();

        utf8Dict.TryGetValue(utf8Key, out utf8DictValue).Should().BeTrue();
        utf8DictValue.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void 存在するキー_最初に追加された参照型アイテムを取得_trueを返す(string key)
    {
        var utf8Dict = new Utf8ArrayDictionary<Test>();
        var utf8Key = new Utf8Array(key);
        var value = new Test(1, "a");

        utf8Dict.TryAdd(utf8Key, value).Should().BeTrue();

        utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeTrue();
        utf8DictValue!.A.Should().Be(value.A);
        utf8DictValue.B.Should().Be(value.B);

        utf8Dict.TryAdd(utf8Key, new(2, "b")).Should().BeFalse();

        utf8Dict.TryGetValue(utf8Key, out utf8DictValue).Should().BeTrue();
        utf8DictValue!.A.Should().Be(value.A);
        utf8DictValue.B.Should().Be(value.B);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void 存在しないキー_falseを返す(string key)
    {
        var utf8Dict = new Utf8ArrayDictionary<int>();
        var utf8Key = new Utf8Array(key);

        utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeFalse();
        utf8DictValue.Should().Be(default);

        utf8Dict.TryGetValue(utf8Key.AsSpan(), out utf8DictValue).Should().BeFalse();
        utf8DictValue.Should().Be(default);

        utf8Dict.TryGetValue(key.AsSpan(), out utf8DictValue).Should().BeFalse();
        utf8DictValue.Should().Be(default);
    }

    sealed class Test
    {
        public Test(int a, string b) => (A, B) = (a, b);

        public int A { get; set; }

        public string B { get; set; }
    }
}
