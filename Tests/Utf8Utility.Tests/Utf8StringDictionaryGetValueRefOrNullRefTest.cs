using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace Utf8Utility.Tests
{
    public sealed class Utf8StringDictionaryGetValueRefOrNullRefTest
    {
        [Fact]
        public void 存在するキー_値型アイテムを取得()
        {
            var utf8Dict = new Utf8StringDictionary<int>();
            var utf8Key = new Utf8String("abc");

            utf8Dict.TryAdd(utf8Key, 1).Should().BeTrue();

            ref var itemRef = ref utf8Dict.GetValueRefOrNullRef(utf8Key);
            itemRef.Should().Be(1);

            itemRef = 2;
            itemRef.Should().Be(2);

            itemRef = ref utf8Dict.GetValueRefOrNullRef(utf8Key.AsSpan());
            itemRef.Should().Be(2);

            itemRef = 3;
            itemRef.Should().Be(3);
        }

        [Fact]
        public void 存在するキー_参照型アイテムを取得()
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

            itemRef = ref utf8Dict.GetValueRefOrNullRef(utf8Key.AsSpan())!;
            itemRef.A.Should().Be(3);
            itemRef.B.Should().Be("c");

            itemRef.A = 4;
            itemRef.B = "d";
            itemRef.A.Should().Be(4);
            itemRef.B.Should().Be("d");

            itemRef = new(5, "e");
            itemRef.A.Should().Be(5);
            itemRef.B.Should().Be("e");
        }

        [Fact]
        public void 存在しないキー_値型アイテムを取得()
        {
            var utf8Dict = new Utf8StringDictionary<int>();
            var utf8Key = new Utf8String("abc");

            Unsafe.IsNullRef(ref utf8Dict.GetValueRefOrNullRef(utf8Key)).Should().BeTrue();
            Unsafe.IsNullRef(ref utf8Dict.GetValueRefOrNullRef(utf8Key.AsSpan())).Should().BeTrue();
        }

        [Fact]
        public void 存在しないキー_参照型アイテムを取得()
        {
            var utf8Dict = new Utf8StringDictionary<Test>();
            var utf8Key = new Utf8String("abc");

            Unsafe.IsNullRef(ref utf8Dict.GetValueRefOrNullRef(utf8Key)).Should().BeTrue();
            Unsafe.IsNullRef(ref utf8Dict.GetValueRefOrNullRef(utf8Key.AsSpan())).Should().BeTrue();
        }

        class Test
        {
            public Test(int a, string b) => (A, B) = (a, b);

            public int A { get; set; }

            public string B { get; set; }
        }
    }
}