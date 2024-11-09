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