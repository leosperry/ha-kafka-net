using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

public abstract record HaEntityStateBase
{
    [JsonPropertyName("entity_id")]
    public virtual required string EntityId { get; init; }
    
    [JsonPropertyName("state")]
    public virtual required string State { get; init; }
    
    [JsonPropertyName("last_changed")]
    public DateTime LastChanged { get; init; }
    
    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; init; }
    
    [JsonPropertyName("context")]
    public HaEventContext? Context { get; init; }
}

public record HaEntityState : HaEntityStateBase
{
    [JsonPropertyName("attributes")]
    public virtual required JsonElement Attributes { get; init; }

    public HaEntityState<T> Convert<T>()
    {
        return new HaEntityState<T>()
        {
            EntityId = this.EntityId,
            State = this.State,
            Attributes = JsonSerializer.Deserialize<T>(this.Attributes),
            Context = this.Context,
        };
    }
}

public record HaEntityState<T> : HaEntityStateBase
{
    [JsonPropertyName("attributes")]
    public virtual required T? Attributes { get; init; }

    

    public static explicit operator HaEntityState<T>(HaEntityState state)
    {
        return new HaEntityState<T>()
        {
            EntityId = state.EntityId,
            State = state.State,
            Attributes = JsonSerializer.Deserialize<T>(state.Attributes),
            Context = state.Context,
        };
    }
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
