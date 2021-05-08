using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utf8Utility.Serialization.Json
{
    /// <summary>
    /// <see cref="System.Text.Json"/>用のカスタムコンバーターです。
    /// </summary>
    public sealed class Utf8StringConverter : JsonConverter<Utf8String>
    {
        /// <inheritdoc/>
        public override Utf8String Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var bytes = reader.HasValueSequence
                ? reader.ValueSequence.ToArray()
                : reader.ValueSpan.ToArray();

            return new Utf8String(bytes);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Utf8String value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.AsSpan());
    }
}