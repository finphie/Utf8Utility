using System;
using FluentAssertions;
using Xunit;

#if NET5_0_OR_GREATER
using System.Text;
#endif

namespace Utf8Utility.Tests
{
    public sealed class Utf8StringIndexerTest
    {
        [Fact]
        public void IntIndex()
        {
            var x = new Utf8String("abc");
            x[0].Should().Be((byte)'a');
            x[1].Should().Be((byte)'b');
            x[2].Should().Be((byte)'c');
        }

        [Fact]
        public void IntIndex_範囲外にアクセス_IndexOutOfRangeExceptionをスローする()
        {
            FluentActions.Invoking(Execute)
                .Should().Throw<IndexOutOfRangeException>();

            static byte Execute()
            {
                var x = new Utf8String("abc");
                return x[10];
            }
        }

#if NET5_0_OR_GREATER
        [Fact]
        public void Index()
        {
            var bytes = Encoding.UTF8.GetBytes("abc");
            var x = new Utf8String(bytes);
            x[^1].ToArray().Should().BeEquivalentTo(bytes[^1]);
        }

        [Fact]
        public void Index_範囲外にアクセス_ArgumentOutOfRangeExceptionをスローする()
        {
            FluentActions.Invoking(Execute)
               .Should().Throw<ArgumentOutOfRangeException>();

            static byte[] Execute()
            {
                var bytes = Encoding.UTF8.GetBytes("abc");
                var x = new Utf8String(bytes);
                return x[^10].ToArray();
            }
        }

        [Fact]
        public void Range()
        {
            var bytes = Encoding.UTF8.GetBytes("abc");
            var x = new Utf8String(bytes);
            x[1..].ToArray().Should().BeEquivalentTo(bytes[1..]);
        }

        [Fact]
        public void Range_範囲外にアクセス_ArgumentOutOfRangeExceptionをスローする()
        {
            FluentActions.Invoking(Execute)
               .Should().Throw<ArgumentOutOfRangeException>();

            static byte[] Execute()
            {
                var bytes = Encoding.UTF8.GetBytes("abc");
                var x = new Utf8String(bytes);
                return x[10..].ToArray();
            }
        }
#endif
    }
}