#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

// https://www.home-assistant.io/integrations/media_player/
[JsonConverter(typeof(JsonStringEnumConverter<MediaPlayerState>))]
public enum MediaPlayerState
{
    Unknown,
    Unavailable,
    Off,
    On,
    Idle,
    Playing,
    Paused,
    Standby,
    Buffering
}