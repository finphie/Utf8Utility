using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Utf8Utility.Serialization.Json.Tests
{
    public sealed class Utf8StringConverterTest
    {
        static readonly JsonSerializerOptions Options = new();

        static Utf8StringConverterTest() => Options.Converters.Add(new Utf8StringConverter());

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        public void Read(string value)
        {
            var json = $"\"{value}\"";

            var deserialized = JsonSerializer.Deserialize<Utf8String>(json, Options);
            deserialized.Should().Be((Utf8String)value);
        }

        [Theory]
        [InlineData("\"\"")]
        [InlineData("\"abc\"")]
        public void Write(string json)
        {
            var deserialized = JsonSerializer.Deserialize<Utf8String>(json, Options);
            var serialized = JsonSerializer.Serialize(deserialized, Options);

            serialized.Should().Be(json);
        }
    }
}