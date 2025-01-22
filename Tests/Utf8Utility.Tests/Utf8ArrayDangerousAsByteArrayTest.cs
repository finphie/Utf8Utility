using System.Text;
using Shouldly;
using Xunit;

namespace Utf8Utility.Tests;

public sealed class Utf8ArrayDangerousAsByteArrayTest
{
    [Theory]
    [InlineData("abcdef")]
    public void 配列を返す(string value)
    {
        var utf8Value = Encoding.UTF8.GetBytes(value);
        var utf8Array = new Utf8Array(utf8Value).DangerousAsByteArray();

        utf8Array.SequenceEqual(utf8Value).ShouldBeTrue();
    }
}
