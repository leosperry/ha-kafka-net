using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

public static class StateExtensions
{
    static JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = 
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new RgbConverter(),
            new RgbwConverter(),
            new RgbwwConverter(),
            new XyConverter(),
            new HsConverter(),
        }
    };

    public static T? GetState<T>(this HaEntityState state)
    {
        try
        {
            //hack
            var stringToDeserialize = typeof(T) == typeof(string) ? $"\"{state.State}\"" : state.State;
            return JsonSerializer.Deserialize<T>(stringToDeserialize);
        }
        catch (System.Exception) 
        {
            // if state is unknown or unavailable, return default
        }
        return default;
    }

    public static T? GetStateEnum<T>(this HaEntityState state) where T: struct, Enum
    {
        try
        {
            return Enum.Parse<T>(state.State!, true);
        }
        catch (System.Exception)
        {
            // if state is unknown or unavailable, return default
        }
        return default;
    }
    public static T? GetAttributes<T>(this HaEntityState state)
    {
        return JsonSerializer.Deserialize<T>(state.Attributes, _options);
    }
    public static T? GetFromAttributes<T>(this HaEntityState state, string key)
    {
        return state.Attributes.GetProperty(key).Deserialize<T>(_options);
    }

    /// <summary>
    /// returns true when state is null or state.State is null or state.State equals "unknown" or "unavailable"
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool Bad<Tstate, Tatt>(this HaEntityState<Tstate, Tatt>? state)
    {
        return state is null || state.State switch
        {
            null => true,
            OnOff onOff => onOff == OnOff.Unknown || onOff == OnOff.Unavailable,
            BatteryState batt => batt == BatteryState.Unknown || batt == BatteryState.Unavailable,
            _ =>  checkForUnknown(state.State)
        };
    }

    private static bool checkForUnknown(object obj)
    {
        var str = obj.ToString();
        return str is null || str == "unknown" || str == "unavailable";
    }
}

