#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace HaKafkaNet;

public abstract record LatLongModel : BaseEntityModel
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
    
}

public record ZoneModel : LatLongModel
{
    [JsonPropertyName("radius")]
    public int radius { get; set; }

    [JsonPropertyName("passive")]
    public bool Passive { get; set; }

    [JsonPropertyName("persons")]
    public required string[] Persons { get; set; }
}

public abstract record TrackerModelBase: LatLongModel
{
    [JsonPropertyName("gps_accuracy")]
    public int? GpsAccuracy { get; set; }
}

public record DeviceTrackerModel : TrackerModelBase
{
    [JsonPropertyName("source_type")]
    public string? SourceType { get; set; }

    [JsonPropertyName("altitude")]
    public int? Altitude { get; set; }
}

public record PersonModel : TrackerModelBase
{
    [JsonPropertyName("source")]
    public required string Source { get; set; }
    
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    
    [JsonPropertyName("device_trackers")]
    public string[]? DeviceTrackers { get; set; }
}
