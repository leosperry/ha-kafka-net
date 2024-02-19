using System.Text.Json.Serialization;

namespace HaKafkaNet;

/// <summary>
/// https://developers.home-assistant.io/docs/core/entity
/// </summary>
public abstract record BaseEntityModel
{
    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }
}

public record DeviceModel : BaseEntityModel
{
    [JsonPropertyName("supported_features")]
    public int? SupportedFeatures { get; init; }

    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; init; }
}

public record SensorModel : DeviceModel
{
    [JsonPropertyName("unit_of_measurement")]
    public string? UnitOfMeasurement { get; set; }
}
