#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace HaKafkaNet;

public record SunModel : HaEntityState<SunState, SunAttributes>
{

} 

[JsonConverter(typeof(JsonStringEnumConverter<SunState>))]
public enum SunState
{
    Unknown,
    Unavailable,
    Above_Horizon, Below_Horizon
}

public record SunAttributes()
{
    [JsonPropertyName("next_dawn")]
    public required DateTime NextDawn { get; init; }

    [JsonPropertyName("next_dusk")]
    public required DateTime NextDusk { get; init; }

    [JsonPropertyName("next_midnight")]
    public required DateTime NextMidnight { get; init; }

    [JsonPropertyName("next_noon")]
    public required DateTime NextNoon { get; init; }

    [JsonPropertyName("next_rising")]
    public required DateTime NextRising { get; init; }

    [JsonPropertyName("next_setting")]
    public required DateTime NextSetting { get; init; }

    [JsonPropertyName("elevation")]
    public required float Elevation { get; init; }

    [JsonPropertyName("azimuth")]
    public required float Azimuth { get; init; }

    [JsonPropertyName("rising")]
    public required bool Rising { get; init; }

    [JsonPropertyName("friendly_name")]
    public required string FriendlyName { get; init; }
}

/// <summary>
/// Used by automation builder and ConsolidatedSunAutomation
/// </summary>
public enum SunEventType
{
    Dawn,
    Rise,
    Noon,
    Set,
    Dusk,
    Midnight
}