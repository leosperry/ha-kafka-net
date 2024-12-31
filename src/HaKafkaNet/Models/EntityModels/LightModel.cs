#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace HaKafkaNet;

public record LightModel : DeviceModel
{
    [JsonPropertyName("brightness")]
    public byte? Brightness { get; init; }

    [JsonPropertyName("color_mode")]
    public string? ColorMode { get; init; }

    /// <summary>
    /// deprecated, but common
    /// </summary>
    [JsonPropertyName("off_with_transition")]
    public bool? OffWithTransition { get; init; }
}

/// <summary>
/// pulled from https://developers.home-assistant.io/docs/core/entity/light/
/// </summary>
public record ColorLightModel : LightModel
{
    [JsonPropertyName("color_temp_kelvin")]
    public int? TempKelvin { get; init; }

    [JsonPropertyName("effect")]
    public string? Effect { get; init; }

    [JsonPropertyName("effect_list")]
    public string[]? EffectList { get; init; }

    [JsonPropertyName("hs_color")]
    public HsColor? HsColor { get; init; }

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; init; }

    [JsonPropertyName("max_color_temp_kelvin")]
    public int? MaxColorTempKelvin { get; init; }

    [JsonPropertyName("min_color_temp_kelvin")]
    public int? Min_ColorTempKelvin { get; init; }

    [JsonPropertyName("rgb_color")]
    public RgbTuple? RGB { get; init; }

    [JsonPropertyName("rgbw_color")]
    public RgbwTuple? RGBW { get; init; }

    [JsonPropertyName("rgbww_color")]
    public RgbwwTuple? RGBWW { get; init; }

    [JsonPropertyName("supported_color_modes")]
    public string[]? SupportedColorModes { get; init; }

    [JsonPropertyName("xy_color")]
    public XyColor? XyColor { get; init; }

    /// <summary>
    /// deprecated, but common
    /// </summary>
    [JsonPropertyName("min_mireds")]
    public int? MinMireds { get; init; }

    /// <summary>
    /// deprecated, but common
    /// </summary>
    [JsonPropertyName("max_mireds")]
    public int? MaxMireds { get; init; }

}
