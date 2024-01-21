using System.Text.Json.Serialization;

namespace HaKafkaNet;

public enum SunState
{
    //[JsonPropertyName("above_horizon")]
    AboveHorizon,
    //[JsonPropertyName("below_horizon")]
    BelowHorizon
}

public record SunAttributes()
{
    [JsonPropertyName("next_dawn")]
    public required DateTime NextDawn { get; init; }

    [JsonPropertyName("next_dusk")]
    public required DateTime NextDusk { get; init; }

    [JsonPropertyName("next_midnight")]
    public required DateTime NexMidnight { get; init; }

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
