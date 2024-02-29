
namespace HaKafkaNet;

public record HaEntityStateChange<T>
{
    public required EventTiming EventTiming { get; set;}

    /// <summary>
    /// Id of the entity which changed state
    /// </summary>
    public required string EntityId { get; set; }
    
    /// <summary>
    /// The most recent item from the cach
    /// </summary>
    public T? Old { get ; set; }

    /// <summary>
    /// new state of the entity
    /// </summary>
    public required T New { get ; set; }
}

public record HaEntityStateChange : HaEntityStateChange<HaEntityState>
{

}
