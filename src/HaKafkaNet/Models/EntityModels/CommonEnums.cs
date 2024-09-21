using System.Text.Json.Serialization;

namespace HaKafkaNet;

[JsonConverter(typeof(JsonStringEnumConverter<OnOff>))]
public enum OnOff
{
    On, Off, Unknown, Unavailable
}

[JsonConverter(typeof(JsonStringEnumConverter<BatteryState>))]
public enum BatteryState
{
    Charging, Discharging, Not_Charging, Unknown, Unavailable
}

/// <summary>
/// For use with media players
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Repeat>))]
public enum Repeat
{
    Off,
    All,
    One
}

