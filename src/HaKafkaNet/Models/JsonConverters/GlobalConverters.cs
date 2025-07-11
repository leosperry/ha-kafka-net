using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet.Models.JsonConverters;

/// <summary>
/// used throughout the framework for JSON serialization 
/// </summary>
public class GlobalConverters
{
    /// <summary>
    /// used throughout the framework for JSON serialization
    /// </summary>
    public static readonly JsonSerializerOptions StandardJsonOptions = new JsonSerializerOptions()
    {
        
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = 
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
            new RgbConverter(),
            new RgbwConverter(),
            new RgbwwConverter(),
            new XyConverter(),
            new HsConverter(),
            new HaDateTimeConverter(),
            new HaNullableDateTimeConverter(),
            new HaDateOnlyConverter(),
            new HaNullableDateOnlyConverter()
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
