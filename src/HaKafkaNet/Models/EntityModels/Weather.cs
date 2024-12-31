#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Text.Json.Serialization;

namespace HaKafkaNet.Models.EntityModels;

/// <summary>
/// https://www.home-assistant.io/integrations/weather/
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<WeatherState>))]
public enum WeatherState
{
    Unknown,
    Unavailable,
    [JsonPropertyName("clear-night")]
    ClearNight,
    Cloudy,
    Fog,
    Hail,
    Lighting,
    [JsonPropertyName("lightning-rainy")]
    LightningRainy,
    [JsonPropertyName("partlycloudy")]
    PartlyCloudy,
    Pouring,
    Rainy,
    Snowy,
    [JsonPropertyName("snowy-rainy")]
    SnowyRainy,
    Sunny,
    Windy,
    [JsonPropertyName("windy-variant")]
    WindyVariant,
    Exceptional
}


public record Weather
{
    [JsonPropertyName("apparent_temperature")]
    public float? ApparentTemperature { get; set; }

    [JsonPropertyName("cloud_coverage")]
    public float? CloudCoverage { get; set; }

    [JsonPropertyName("dew_point")]
    public float? DewDoint { get; set; }

    [JsonPropertyName("humidity")]
    public float? Humidity { get; set; }

    [JsonPropertyName("precipitation_unit")]
    public string? PrecipitationUnit { get; set; }

    [JsonPropertyName("pressure")]
    public float? Pressure { get; set; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    [JsonPropertyName("temperature_unit")]
    public string? TemperatureUnit { get; set; }

    [JsonPropertyName("uv_index")]
    public float? UV_Index { get; set; }

    [JsonPropertyName("visibility")]
    public float? Visibility { get; set; }

    [JsonPropertyName("wind_bearing")]
    public float? wind_bearing { get; set; }

    [JsonPropertyName("wind_gust_speed")]
    public float? WindGustSpeed { get; set; }

    [JsonPropertyName("wind_speed")]
    public float? WindSpeed { get; set; }

    [JsonPropertyName("wind_speed_unit")]
    public string? WindSpeedUnit { get; set; }
}
