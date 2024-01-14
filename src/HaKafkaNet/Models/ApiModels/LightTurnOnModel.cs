global using RgbTuple = (byte red, byte green, byte blue);
global using RgbwTuple = (byte red, byte green, byte blue, byte white);
global using RgbwwTuple = (byte red, byte green, byte blue, byte white, byte warmWhite);
global using XyColor = (float x, float y);
global using HsColor = (float hue, float saturation);

using System.Text.Json.Serialization;

namespace HaKafkaNet;

/// <summary>
/// A model representing the settings you can use when adjusting lights
/// Most of this documentation was pulled directly from 
/// https://www.home-assistant.io/integrations/light/
/// </summary>
public record LightTurnOnModel
{
    [JsonPropertyName("entity_id")]
    public required string EntityId { get; init; }

    /// <summary>
    /// Number that represents the time (in seconds) the light should take to transition to the new state.
    /// </summary>
    [JsonPropertyName("transition")]
    public int? Transition { get; set; }
 
    /// <summary>
    /// String with the name of one of the built-in profiles (relax, energize, concentrate, reading) or one of the custom profiles defined in light_profiles.csv
    /// </summary>
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    /// <summary>
    /// A list containing two floats representing the hue and saturation of the color you want the light to be. Hue is scaled 0-360, and saturation is scaled 0-100.
    /// </summary>
    [JsonPropertyName("hs_color")]
    public HsColor? HueSat { get; set; }

    /// <summary>
    /// A list containing two floats representing the xy color you want the light to be. Two comma-separated floats that represent the color in XY
    /// </summary>
    [JsonPropertyName("xy_color")]
    public XyColor? XyColor { get; set; }

    /// <summary>
    /// A list containing three integers between 0 and 255 representing the RGB color you want the light to be. Three comma-separated integers that represent the color in RGB, within square brackets.
    /// </summary>
    [JsonPropertyName("rgb_color")]
    public RgbTuple? RgbColor { get; set; }
    
    /// <summary>
    /// A list containing four integers between 0 and 255 representing the RGBW color you want the light to be. Four comma-separated integers that represent the color in RGBW (red, green, blue, white), within square brackets. This attribute will be ignored by lights which do not support RGBW colors.
    /// </summary>
    [JsonPropertyName("rgbw_color")]
    public RgbwTuple? RgbwColor { get; set; }
    
    /// <summary>
    /// A list containing five integers between 0 and 255 representing the RGBWW color you want the light to be. Five comma-separated integers that represent the color in RGBWW (red, green, blue, cold white, warm white), within square brackets. This attribute will be ignored by lights which do not support RGBWW colors.
    /// </summary>
    [JsonPropertyName("rgbww_color")]
    public RgbwwTuple? RgbwwColor { get; set; }

    /// <summary>
    /// An integer in Kelvin representing the color temperature you want the light to be
    /// </summary>
    [JsonPropertyName("color_temp_kelvin")]
    public int? Kelvin { get; set; }

    /// <summary>
    /// A human-readable string of a color name, such as blue or goldenrod. All CSS3 color names are supported.
    /// </summary>
    [JsonPropertyName("color_name")]
    public string? ColorName { get; init; }

    /// <summary>
    /// Integer between 0 and 255 for how bright the light should be, where 0 means the light is off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light
    /// </summary>
    [JsonPropertyName("brightness")]
    public byte? Brightness { get; init; }

    /// <summary>
    /// specify brightness in percent (a number between 0 and 100), where 0 means the light is off, 1 is the minimum brightness and 100 is the maximum brightness supported by the light.
    /// </summary>
    [JsonPropertyName("brightness_pct")]
    public int? BrightnessPct { get; init; }

    /// <summary>
    /// Change brightness by an amount. Should be between -255..255.
    /// </summary>
    [JsonPropertyName("brightness_step")]
    public int? BrightnessStep { get; init; }

    /// <summary>
    /// Change brightness by a percentage. Should be between -100..100.
    /// </summary>
    [JsonPropertyName("brightness_step_pct")]
    public int? BrightnessStepPctt { get; init; }

    
    /// <summary>
    /// Tell light to flash, can be either value short or long.
    /// </summary>
    [JsonPropertyName("flash")]
    public Flash? Flash {get; init;}

    /// <summary>
    /// Tell light to flash, can be either value short or long.
    /// </summary>
    [JsonPropertyName("effect")]
    public string? Effect {get; init;}
}

public enum Flash
{
    Long, Short
}

