using System.Text.Json.Serialization;

namespace HaKafkaNet;

[JsonConverter(typeof(JsonStringEnumConverter<OnOff>))]
public enum OnOff
{
    Unknown, 
    Unavailable,
    On, 
    Off
}

[JsonConverter(typeof(JsonStringEnumConverter<BatteryState>))]
public enum BatteryState
{
    Unknown, 
    Unavailable,
    Charging, 
    Discharging, 
    Not_Charging
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

