#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Text.Json.Serialization;

namespace HaKafkaNet;

/// <summary>
/// https://github.com/leosperry/ha-kafka-net/wiki/Scene-Controllers
/// </summary>
public record SceneControllerEvent : BaseEntityModel
{
    [JsonPropertyName("event_types")]
    public string[]? EventTypes { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
}

public static class EventModelExtensions
{
    /// <summary>
    /// see: https://github.com/leosperry/ha-kafka-net/wiki/Scene-Controllers
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static KeyPress? GetKeyPress(this SceneControllerEvent model)
    {
        if (model.EventType is not null && Enum.TryParse<KeyPress>(model.EventType, out var keyPress))
        {
            return keyPress;
        }
        return null;
    } 
}

public enum KeyPress
{
    KeyHeldDown, 
    KeyPressed, 
    KeyPressed2x, 
    KeyPressed3x, 
    KeyPressed4x, 
    KeyPressed5x, 
    KeyReleased
}
