using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests
{
    public sealed class Utf8StringDictionaryGetValueRefOrNullRefTest
    {
        [Fact]
        public void 存在するキーの値型アイテムを取得()
        {
            var utf8Dict = new Utf8StringDictionary<int>();
            var utf8Key = new Utf8String("abc");

            utf8Dict.TryAdd(utf8Key, 1).Should().BeTrue();

            ref var itemRef = ref utf8Dict.GetValueRefOrNullRef(utf8Key);
            itemRef.Should().Be(1);

            itemRef = 2;
            itemRef.Should().Be(2);
        }

        [Fact]
        public void 存在するキーの参照型アイテムを取得()
        {
            var utf8Dict = new Utf8StringDictionary<Test>();
            var utf8Key = new Utf8String("abc");

            utf8Dict.TryAdd(utf8Key, new(1, "a")).Should().BeTrue();

            ref var itemRef = ref utf8Dict.GetValueRefOrNullRef(utf8Key);
            itemRef.A.Should().Be(1);
            itemRef.B.Should().Be("a");

            itemRef.A = 2;
            itemRef.B = "b";
            itemRef.A.Should().Be(2);
            itemRef.B.Should().Be("b");

            itemRef = new(3, "c");
            itemRef.A.Should().Be(3);
            itemRef.B.Should().Be("c");
        }

        [Fact]
        public void 存在しないキーの値型アイテムを取得()
        {
            var utf8Dict = new Utf8StringDictionary<int>();
            var utf8Key = new Utf8String("abc");

            Unsafe.IsNullRef(ref utf8Dict.GetValueRefOrNullRef(utf8Key)).Should().BeTrue();
        }

        [Fact]
        public void 存在しないキーの参照型アイテムを取得()
        {
            var utf8Dict = new Utf8StringDictionary<Test>();
            var utf8Key = new Utf8String("abc");

            Unsafe.IsNullRef(ref utf8Dict.GetValueRefOrNullRef(utf8Key)).Should().BeTrue();
        }

        class Test
        {
            public Test(int a, string b) => (A, B) = (a, b);

            public int A { get; set; }

            public string B { get; set; }
        }
    }
}