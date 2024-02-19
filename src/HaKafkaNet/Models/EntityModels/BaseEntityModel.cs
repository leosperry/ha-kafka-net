using System.Text.Json.Serialization;

namespace HaKafkaNet;

/// <summary>
/// https://developers.home-assistant.io/docs/core/entity
/// </summary>
public abstract record BaseEntityModel
{
    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }
}

public abstract record BaseDeviceModel : BaseEntityModel
{
    [JsonPropertyName("supported_features")]
    public int? SupportedFeatures { get; init; }

    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; init; }

    [JsonPropertyName("icon ")]
    public string? Icon { get; init; }
}
