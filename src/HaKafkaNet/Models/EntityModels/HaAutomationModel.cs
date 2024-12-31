#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace HaKafkaNet;

public record HaAutomationModel : BaseEntityModel
{
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    [JsonPropertyName("last_triggered")]
    public DateTime? LastTriggered { get; set; }

    [JsonPropertyName("mode")]
    [JsonConverter(typeof(JsonStringEnumConverter<HaAutomationMode>))]
    public HaAutomationMode Mode { get; set; }

    [JsonPropertyName("current")]
    public int Current { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<HaAutomationMode>))]
public enum HaAutomationMode
{
    Single, Restart, Queued, Parallel
}
