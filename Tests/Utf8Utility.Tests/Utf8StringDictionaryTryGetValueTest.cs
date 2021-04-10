using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests
{
    public sealed class Utf8StringDictionaryTryGetValueTest
    {
        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        public void 存在するキーを追加_trueを返す(string key)
        {
            var utf8Dict = new Utf8StringDictionary<int>();
            var utf8Key = new Utf8String(key);
            utf8Dict.Add(utf8Key, 1);

            utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeTrue();
            utf8DictValue.Should().Be(1);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        public void 存在するキーを2回追加_trueを返す(string key)
        {
            var utf8Dict = new Utf8StringDictionary<int>();
            var utf8Key = new Utf8String(key);

            for (var i = 0; i < 2; i++)
            {
                utf8Dict.Add(utf8Key, 1);

                utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeTrue();
                utf8DictValue.Should().Be(1);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        public void 存在しないキー_falseを返す(string key)
        {
            var utf8Dict = new Utf8StringDictionary<int>();
            var utf8Key = new Utf8String(key);
            utf8Dict.TryGetValue(utf8Key, out var utf8DictValue).Should().BeFalse();
            utf8DictValue.Should().Be(default);
        }
    }
}