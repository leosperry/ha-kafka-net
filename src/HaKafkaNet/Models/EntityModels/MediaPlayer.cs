using System;
using System.Text.Json.Serialization;

namespace HaKafkaNet.Models.EntityModels;

// https://www.home-assistant.io/integrations/media_player/
[JsonConverter(typeof(JsonStringEnumConverter<MediaPlayerState>))]
public enum MediaPlayerState
{
    Off,
    On,
    Idle,
    Playing,
    Paused,
    Standby,
    Buffering,
    Unknown,
    Unavailable
}