
namespace HaKafkaNet;

/// <summary>
/// represents an entity changing state
/// </summary>
/// <typeparam name="T"></typeparam>
public record HaEntityStateChange<T>
{
    /// <summary>
    /// The timing assigned by the framework
    /// see: https://github.com/leosperry/ha-kafka-net/wiki/Event-Timings
    /// </summary>
    public required EventTiming EventTiming { get; set;}

    /// <summary>
    /// Id of the entity which changed state
    /// </summary>
    public required string EntityId { get; set; }
    
    /// <summary>
    /// The most recent item from the cache
    /// </summary>
    public T? Old { get ; set; }

    /// <summary>
    /// new state of the entity
    /// </summary>
    public required T New { get ; set; }
}

/// <summary>
/// represents an entity changing state in raw form
/// </summary>
public record HaEntityStateChange : HaEntityStateChange<HaEntityState>;

