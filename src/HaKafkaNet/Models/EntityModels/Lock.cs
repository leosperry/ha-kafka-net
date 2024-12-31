#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
