using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HaKafkaNet;

public abstract record HaEntityStateBase
{    
    [JsonPropertyName("entity_id")]
    public virtual required string EntityId { get; init; }

    [JsonPropertyName("last_changed")]
    public DateTime LastChanged { get; init; }
    
    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; init; }
    
    [JsonPropertyName("context")]
    public HaEventContext? Context { get; init; }
}

public record HaEntityState<Tstate, Tattributes> : HaEntityStateBase
{
    [JsonPropertyName("state")]
    public virtual required Tstate State { get; init; }

    [JsonPropertyName("attributes")]
    public Tattributes? Attributes { get; init; }

    public static explicit operator HaEntityState<Tstate, Tattributes>(HaEntityState state)
    {
        Func<Tstate> stateGetter = () => {

            Tstate? retVal;
            bool isNullable = Nullable.GetUnderlyingType(typeof(Tstate)) != null;

            if (typeof(Tstate).IsEnum)
            {
                var newVal = (Tstate)Enum.Parse(typeof(Tstate), state.State, true);
                return newVal;
            }

            if (isNullable && Nullable.GetUnderlyingType(typeof(Tstate))!.IsEnum)
            {
                try
                {
                    retVal = (Tstate)Enum.Parse(typeof(Tstate), state.State, true);
                }
                catch
                {
                    // couldn't parse it, make it null
                    retVal = default;
                }
                return retVal!;
            }

            // now handle non-enum
            var parsed = state.GetState<Tstate>();
            if (parsed is null)
            {
                if (IsNullable(typeof(Tstate)))
                {
                    return parsed!;
                }
                else
                {
                    throw new HaKafkaNetException("could not pars non-nullable state");
                }
            }
            return parsed!;
        };
        var attributes = state.GetAttributes<Tattributes>() ?? throw new HaKafkaNetException("could not convert attributes");
        var newState = stateGetter();
        return new()
        {
            EntityId = state.EntityId,
            State = newState,
            Attributes = attributes,
            Context = state.Context,
            LastChanged = state.LastChanged,
            LastUpdated = state.LastUpdated
        };
    }    

    static bool IsNullable(Type type) => Nullable.GetUnderlyingType(type) != null;
}

public record HaEntityState : HaEntityState<string, JsonElement>
{
    [Obsolete("please use Attributes extension method", true)]
    public HaEntityState<T> Convert<T>()
    {
        return new HaEntityState<T>()
        {
            EntityId = this.EntityId,
            State = this.State,
            Attributes = JsonSerializer.Deserialize<T>(this.Attributes) ?? throw new HaKafkaNetException("could not convert attributes"),
            Context = this.Context,
        };
    }
}

[Obsolete("please use HaEntityState<string, Tatt>", false)]
public record HaEntityState<T> : HaEntityState<string, T> { }

public record HaEventContext
{
    [JsonPropertyName("id")]
    public required string ID { get; init; }
    
    [JsonPropertyName("parent_id")]
    public string? ParentId { get; init; }
    
    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }
}

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
