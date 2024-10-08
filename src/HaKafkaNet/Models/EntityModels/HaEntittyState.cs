
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

public record HaEntityState<Tstate, Tattributes> : IHaEntity<Tstate, Tattributes>
{    
    [JsonPropertyName("entity_id")]
    public required string EntityId { get; init; }

    [JsonPropertyName("last_changed")]
    public DateTime LastChanged { get; init; }
    
    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; init; }
    
    [JsonPropertyName("context")]
    public HaEventContext? Context { get; init; }

    [JsonPropertyName("state")]
    public required Tstate State { get; init; }

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

public record HaEntityState : HaEntityState<string, JsonElement>, IHaEntity
{
    /// <summary>
    /// Used internally for startup EventTiming.PreStartupSameAsLastCached
    /// for a state change, change.Old should be same as change.New.Previous
    /// except for pre-startup pre cached
    /// Will be null if fetched from IHaApi
    /// </summary>
    public HaEntityState? Previous{ get; internal set; }
}


public record HaEventContext
{
    [JsonPropertyName("id")]
    public required string ID { get; init; }
    
    [JsonPropertyName("parent_id")]
    public string? ParentId { get; init; }
    
    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }
}

