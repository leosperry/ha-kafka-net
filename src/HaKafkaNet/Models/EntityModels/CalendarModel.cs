#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace HaKafkaNet;

public record CalendarModel : BaseEntityModel
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("all_day")]
    public bool? AllDay { get; set; }

    [JsonPropertyName("start_time")]
    public DateTime? StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public DateTime? EndTime { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

}
