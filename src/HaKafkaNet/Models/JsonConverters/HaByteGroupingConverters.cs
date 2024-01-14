using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

public class RgbConverter : JsonConverter<RgbTuple> 
{
    public override RgbTuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = JsonSerializer.Deserialize<byte[]>(reader.GetString()!)!;
        RgbTuple retVal = (bytes[0], bytes[1], bytes[2]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, RgbTuple value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.red);
        writer.WriteNumberValue(value.green);
        writer.WriteNumberValue(value.blue);
        writer.WriteEndArray();
    }
}

public class RgbwConverter : JsonConverter<RgbwTuple> 
{
    public override RgbwTuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = JsonSerializer.Deserialize<byte[]>(reader.GetString()!)!;
        RgbwTuple retVal = (bytes[0], bytes[1], bytes[2], bytes[3]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, RgbwTuple value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.red);
        writer.WriteNumberValue(value.green);
        writer.WriteNumberValue(value.blue);
        writer.WriteNumberValue(value.white);
        writer.WriteEndArray();
    }
}

public class RgbwwConverter : JsonConverter<RgbwwTuple> 
{
    public override RgbwwTuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = JsonSerializer.Deserialize<byte[]>(reader.GetString()!)!;
        RgbwwTuple retVal = (bytes[0], bytes[1], bytes[2], bytes[3], bytes[4]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, RgbwwTuple value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.red);
        writer.WriteNumberValue(value.green);
        writer.WriteNumberValue(value.blue);
        writer.WriteNumberValue(value.white);
        writer.WriteNumberValue(value.warmWhite);
        writer.WriteEndArray();
    }
}

public class XyConverter : JsonConverter<XyColor> 
{
    public override XyColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = JsonSerializer.Deserialize<float[]>(reader.GetString()!)!;
        XyColor retVal = (bytes[0], bytes[1]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, XyColor value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.x);
        writer.WriteNumberValue(value.y);
        writer.WriteEndArray();
    }
}

public class HsConverter : JsonConverter<HsColor> 
{
    public override HsColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = JsonSerializer.Deserialize<float[]>(reader.GetString()!)!;
        HsColor retVal = (bytes[0], bytes[1]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, HsColor value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.hue);
        writer.WriteNumberValue(value.saturation);
        writer.WriteEndArray();
    }
}


