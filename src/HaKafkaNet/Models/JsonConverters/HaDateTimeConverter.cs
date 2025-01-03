#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

public class HaNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return DateTime.ParseExact(reader.GetString() ?? string.Empty, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.AssumeLocal);
        }
        catch (System.Exception)
        {
            // swallow it
        }
        // this should suffice for date only
        return DateTime.Parse(reader.GetString() ?? string.Empty, null, DateTimeStyles.AssumeLocal);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
public class HaDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return DateTime.ParseExact(reader.GetString() ?? string.Empty, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.AssumeLocal);
        }
        catch (System.Exception)
        {
            // swallow it
        }
        // this should suffice for date only
        return DateTime.Parse(reader.GetString() ?? string.Empty, null, DateTimeStyles.AssumeLocal);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}


public class HaDateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return DateOnly.ParseExact(reader.GetString() ?? string.Empty, "yyyy-MM-dd", null);
        }
        catch (System.Exception)
        {
            // swallow it
        }
        return DateOnly.ParseExact(reader.GetString()?.Split(' ')[0] ?? string.Empty, "yyyy-MM-dd", null);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class HaNullableDateOnlyConverter : JsonConverter<DateOnly?>
{
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return DateOnly.ParseExact(reader.GetString() ?? string.Empty, "yyyy-MM-dd", null);
        }
        catch (System.Exception)
        {
            // swallow it
        }
        return DateOnly.ParseExact(reader.GetString()?.Split(' ')[0] ?? string.Empty, "yyyy-MM-dd", null);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}



