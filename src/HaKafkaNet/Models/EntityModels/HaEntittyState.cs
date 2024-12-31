using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

/// <summary>
/// strongly typed version of HA state
/// </summary>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tattributes"></typeparam>
public record HaEntityState<Tstate, Tattributes> : IHaEntity<Tstate, Tattributes>
{    
    /// <summary>
    /// id defined in HA
    /// </summary>
    [JsonPropertyName("entity_id")]
    public required string EntityId { get; init; }

    /// <summary>
    /// only updates on state changes
    /// </summary>
    [JsonPropertyName("last_changed")]
    public DateTime LastChanged { get; init; }
    
    /// <summary>
    /// updates when attributes or state changes
    /// </summary>
    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; init; }
    
    /// <summary>
    /// used by HA
    /// </summary>
    [JsonPropertyName("context")]
    public HaEventContext? Context { get; init; }

    /// <summary>
    /// The state of the entity. Could be unknown or unavailable
    /// </summary>
    [JsonPropertyName("state")]
    public required Tstate State { get; init; }

    /// <summary>
    /// Attributes defined by an integration in HA
    /// </summary>
    [JsonPropertyName("attributes")]
    public Tattributes? Attributes { get; init; }

    /// <summary>
    /// used for converting raw HA state to strongly typed state
    /// </summary>
    /// <param name="state"></param>
    public static explicit operator HaEntityState<Tstate, Tattributes>(HaEntityState state)
    {
        var newState = GetState(state);
        var newAttributes = state.GetAttributes<Tattributes>();

        return new()
        {
            EntityId = state.EntityId,
            State = newState,
            Attributes = newAttributes,
            Context = state.Context,
            LastChanged = state.LastChanged,
            LastUpdated = state.LastUpdated,
        };
    }

    private static Tstate GetState(HaEntityState state)
    {
        // handle enum value type e.g. OnOff
        if (typeof(Tstate).IsEnum)
        {
            var newVal = (Tstate)Enum.Parse(typeof(Tstate), state.State, true);
            return newVal;
        }

        var nullableUnderlyingType = Nullable.GetUnderlyingType(typeof(Tstate));
        bool isNullableValueType = nullableUnderlyingType is not null;

        // handle nullable enums
        if (isNullableValueType && nullableUnderlyingType!.IsEnum)
        {
            Tstate? retVal;
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
        try
        {
            // this line could throw exception
            var parsed = state.GetState<Tstate>();

            return parsed!;
        }
        catch (System.Exception)
        {
            if (isNullableValueType)
            {
                // return null
                return default!;
            }
            throw;
        }
    }
}

/// <summary>
/// All state changes from HA come into the system as this model
/// </summary>
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public record HaEventContext
{
    [JsonPropertyName("id")]
    public required string ID { get; init; }
    
    [JsonPropertyName("parent_id")]
    public string? ParentId { get; init; }
    
    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }
}

