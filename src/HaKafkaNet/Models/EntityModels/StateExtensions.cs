using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using HaKafkaNet.Models.JsonConverters;

namespace HaKafkaNet;

/// <summary>
/// 
/// </summary>
public static class StateExtensions
{
    static JsonSerializerOptions _options = GlobalConverters.StandardJsonOptions;

    /// <summary>
    /// Gets a typed state from the state property of a raw state object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public static T? GetState<T>(this HaEntityState state)
    {
        if (state.State is T str) return str; // leave it alone if string
        const char quote = '"';
        ReadOnlySpan<char> wrapped = new ReadOnlySpan<char>([quote, ..state.State.ToArray(), quote]);
        return JsonSerializer.Deserialize<T>(wrapped, _options);
    }

    /// <summary>
    /// gets an enum from the state property of a raw state object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public static T? GetStateEnum<T>(this HaEntityState state) where T: struct, Enum
    {
        try
        {
            return Enum.Parse<T>(state.State, true);
        }
        catch (System.Exception)
        {
            // if state is unknown or unavailable, return null
        }
        return null;
    }

    /// <summary>
    /// gets typed attributes from a raw state object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public static T? GetAttributes<T>(this HaEntityState state)
    {
        if(state.Attributes is T typed) return typed; // if it's JsonElement leave it alone
        return JsonSerializer.Deserialize<T>(state.Attributes, _options);
    }

    /// <summary>
    /// gets typed property from the attributes from a raw state object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="state"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T? GetFromAttributes<T>(this HaEntityState state, string key)
    {
        return state.Attributes.GetProperty(key).Deserialize<T>(_options);
    }

    /// <summary>
    /// gets a typed property from JsonElement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="key"></param>
    /// <returns></returns>
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
    /// sometimes scene controllers update state, but weren't actually pressed
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool StateAndLastUpdatedWithin1Second<_>([AllowNull]this IHaEntity<DateTime?, _> state)
    {
        var diff = state?.State - state?.LastUpdated;
        return diff is not null && Math.Abs(diff.Value.TotalSeconds) < 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="_"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool IsOn<_>([AllowNull][NotNullWhen(true)]this IHaEntity<OnOff, _> state)
        => state?.State == OnOff.On;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="_"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool IsOff<_>([AllowNull][NotNullWhen(true)]this IHaEntity<OnOff, _> state)
        => state?.State == OnOff.Off;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="_"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool IsHome<_>([AllowNull]this IHaEntity<string, _> state) where _ : TrackerModelBase
    {
        return state?.State == "home";
    }

    /// <summary>
    /// Gets the friendly name from a raw state if it is defined otherwise "name not specified"
    /// </summary>
    /// <typeparam name="_"></typeparam>
    /// <param name="state"></param>
    /// <param name="fallback"></param>
    /// <returns></returns>
    public static string FriendlyName<_>(this IHaEntity<_, JsonElement> state, string? fallback = null)
    {
        return state.Attributes.GetProperty("friendly_name").GetString() ?? fallback ?? "name not specified";
    }
}

