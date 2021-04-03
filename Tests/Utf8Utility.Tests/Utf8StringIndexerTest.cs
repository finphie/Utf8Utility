using System.Text;
using FluentAssertions;
using Xunit;

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

#if NET5_0_OR_GREATER
        [Fact]
        public void Index()
        {
            var bytes = Encoding.UTF8.GetBytes("abc");
            var x = new Utf8String(bytes);
            x[^1].ToArray().Should().BeEquivalentTo(bytes[^1]);
        }

        [Fact]
        public void Range()
        {
            var bytes = Encoding.UTF8.GetBytes("abc");
            var x = new Utf8String(bytes);
            x[1..].ToArray().Should().BeEquivalentTo(bytes[1..]);
        }
#endif
    }
}