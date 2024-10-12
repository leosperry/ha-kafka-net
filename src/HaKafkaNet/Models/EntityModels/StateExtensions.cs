using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using HaKafkaNet.Models.JsonConverters;

namespace HaKafkaNet;

public static class StateExtensions
{
    static JsonSerializerOptions _options = GlobalConverters.StandardJsonOptions;

    public static T? GetState<T>(this HaEntityState state)
    {
        try
        {
            var stringToDeserialize = $"\"{state.State}\"";
            return JsonSerializer.Deserialize<T>(stringToDeserialize, _options);
        }
        catch (Exception) 
        {
            //System.Console.WriteLine(ex);
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

    public static T? GetFromElement<T>(this JsonElement element, string key)
    {
        return element.GetProperty(key).Deserialize<T>(_options);
    }

    /// <summary>
    /// returns true when state is null or state.State is null or state.State equals "unknown" or "unavailable"
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool Bad<Tstate, Tatt>([NotNullWhen(false)][AllowNull]this IHaEntity<Tstate, Tatt>? state)
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
        return str is null || str.Equals("unknown", StringComparison.OrdinalIgnoreCase) || str.Equals("unavailable", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// returns true if the state and last updated times have less than one second difference.
    /// sometimes scene controllers update state, but wern't actually pressed
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool StateAndLastUpdatedWithin1Second<_>([AllowNull]this IHaEntity<DateTime?, _> state)
    {
        var diff = state?.State - state?.LastUpdated;
        return diff is not null && Math.Abs(diff.Value.TotalSeconds) < 1;
    }

    public static bool IsOn<_>([AllowNull][NotNullWhen(true)]this IHaEntity<OnOff, _> state)
        => state?.State == OnOff.On;

        public static bool IsOff<_>([AllowNull][NotNullWhen(true)]this IHaEntity<OnOff, _> state)
        => state?.State == OnOff.Off;

    public static bool IsHome<_>([AllowNull]this IHaEntity<string, _> state) where _ : TrackerModelBase
    {
        return state?.State == "home";
    }
}

