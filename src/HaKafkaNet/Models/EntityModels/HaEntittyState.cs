using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public virtual required Tattributes Attributes { get; init; }

    public static explicit operator HaEntityState<Tstate, Tattributes>(HaEntityState state)
    {
        Func<Tstate> stateGetter = () =>{
            if (typeof(Tstate).IsAssignableFrom(typeof(Enum)))
            {
                return (Tstate)Enum.Parse(typeof(Tstate), state.State);
            }
            return state.State<Tstate>();
        };
        var attributes = state.Attributes<Tattributes>() ?? throw new HaKafkaNetException("could not convert attributes");
        return new()
        {
            EntityId = state.EntityId,
            State = stateGetter(),
            Attributes = attributes,
            Context = state.Context,
            LastChanged = state.LastChanged,
            LastUpdated = state.LastUpdated
        };
    }    
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


[Obsolete("please use HaEntityState<Tstate, Tatt>", false)]
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
    public static T State<T>(this HaEntityState state) =>  JsonSerializer.Deserialize<T>(state.State) ?? throw new HaKafkaNetException("could not deserialize state");
    public static T StateEnum<T>(this HaEntityState state) where T: struct, Enum => Enum.Parse<T>(state.State, true);
    public static T? Attributes<T>(this HaEntityState state) => JsonSerializer.Deserialize<T>(state.Attributes);
    public static T? FromAttributes<T>(this HaEntityState state, string key) => state.Attributes.GetProperty(key).Deserialize<T>();
}
