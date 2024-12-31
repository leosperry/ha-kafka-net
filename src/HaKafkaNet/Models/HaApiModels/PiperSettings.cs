#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace HaKafkaNet;

/// <summary>
/// https://github.com/home-assistant/addons/blob/master/piper/DOCS.md
/// </summary>
public record PiperSettings
{
    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    [JsonPropertyName("speaker")]
    public int Speaker { get; set; }

    [JsonPropertyName("length_scale")]
    public float? LengthScale { get; set; }

    [JsonPropertyName("noise_scale")]
    public float? NoiseScale { get; set; }

    [JsonPropertyName("noise_w")]
    public float? SpeakingCadence { get; set; }
}
