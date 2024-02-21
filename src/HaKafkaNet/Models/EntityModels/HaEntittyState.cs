using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HaKafkaNet;

public record HaEntityState<Tstate, Tattributes>
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

