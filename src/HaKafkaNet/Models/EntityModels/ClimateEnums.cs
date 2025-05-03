#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

[JsonConverter(typeof(JsonStringEnumConverter<HvacMode>))]
public enum HvacMode
{
    Unknown, 
    Unavailable,
    Off,
    Heat,
    Cool,
    [JsonPropertyName("heat_cool")]
    HeatCool,
    Auto,
    Dry,
    [JsonPropertyName("fan_only")]
    FanOnly
}

[JsonConverter(typeof(JsonStringEnumConverter<CarrierFanMode>))]
public enum CarrierFanMode
{
    Unknown, 
    Unavailable,
    Low,
    med,
    High,
    Auto
}

[JsonConverter(typeof(JsonStringEnumConverter<CarrierPresetMode>))]
public enum CarrierPresetMode
{
    Unknown, 
    Unavailable,
    Away,
    Home,
    manual,
    Sleep,
    wake,
    vacation,
    resume
}