#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

public class HaDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());

    }
}
