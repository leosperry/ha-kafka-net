using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

public class RgbConverter : JsonConverter<RgbTuple?> 
{
    public override bool HandleNull => true;

    public override RgbTuple? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = JsonSerializer.Deserialize<int[]?>(ref reader);
        if (bytes is null) return null;
        RgbTuple retVal = new RgbTuple((byte)bytes[0], (byte)bytes[1], (byte)bytes[2]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, RgbTuple? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Red);
            writer.WriteNumberValue(value.Green);
            writer.WriteNumberValue(value.Blue);
            writer.WriteEndArray();
        }
    }
}

public class RgbwConverter : JsonConverter<RgbwTuple?> 
{
    public override bool HandleNull => true;

    public override RgbwTuple? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = JsonSerializer.Deserialize<int[]?>(ref reader);
        if (bytes is null) return null;
        RgbwTuple retVal = new RgbwTuple((byte)bytes[0], (byte)bytes[1], (byte)bytes[2], (byte)bytes[3]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, RgbwTuple? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Red);
            writer.WriteNumberValue(value.Green);
            writer.WriteNumberValue(value.Blue);
            writer.WriteNumberValue(value.White);
            writer.WriteEndArray();
        }
    }
}

public class RgbwwConverter : JsonConverter<RgbwwTuple?> 
{
    public override bool HandleNull => true;
    
    public override RgbwwTuple? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = JsonSerializer.Deserialize<int[]?>(ref reader);
        if (bytes is null) return null;
        RgbwwTuple retVal = new((byte)bytes[0], (byte)bytes[1], (byte)bytes[2], (byte)bytes[3], (byte)bytes[4]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, RgbwwTuple? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Red);
            writer.WriteNumberValue(value.Green);
            writer.WriteNumberValue(value.Blue);
            writer.WriteNumberValue(value.White);
            writer.WriteNumberValue(value.WarmWhite);
            writer.WriteEndArray();
        }
    }
}

public class XyConverter : JsonConverter<XyColor?> 
{
    public override bool HandleNull => true;
    
    public override XyColor? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var floats = JsonSerializer.Deserialize<float[]?>(ref reader);
        if (floats is null) return null;
        XyColor retVal = new(floats[0], floats[1]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, XyColor? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}

public class HsConverter : JsonConverter<HsColor?> 
{
    public override bool HandleNull => true;
    
    public override HsColor? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var floats = JsonSerializer.Deserialize<float[]?>(ref reader);
        if (floats is null) return null;
        HsColor retVal = new(floats[0], floats[1]);
        return retVal;
    }

    public override void Write(Utf8JsonWriter writer, HsColor? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Hue);
            writer.WriteNumberValue(value.Saturation);
            writer.WriteEndArray();
        }
    }
}


