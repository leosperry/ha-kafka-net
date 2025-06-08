using System;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

public record TagAttributes
{
    [JsonPropertyName("tag_id")]
    public required string TagId { get; set; }
    [JsonPropertyName("last_scanned_by_device_id")]
    public required string LastScannedByDeviceId { get; set; }
    [JsonPropertyName("friendly_name")]
    public required string FriendlyName { get; set; }
}
