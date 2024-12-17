using System;
using System.Text.Json.Serialization;

namespace HaKafkaNet.Models.EntityModels;

/// <summary>
/// https://www.home-assistant.io/integrations/lock/
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<LockState>))]
public enum LockState
{
    Unknown,
    Unavailable,
    Jammed,
    Open,
    Opening,
    Locked,
    Locking,
    Unlocking,
}
