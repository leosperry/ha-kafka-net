namespace HaKafkaNet;

public record HaEntityStateChange
{
    public required EventTiming EventTiming { get; set;}

    /// <summary>
    /// Id of the entity which changed state
    /// </summary>
    public required string EntityId { get; set; }

    /// <summary>
    /// The most recent item from the cach before the state change occurred
    /// </summary>
    public HaEntityState? Old { get; set; }

    /// <summary>
    /// new state of the entity
    /// </summary>
    public required HaEntityState New { get; init; }
    
}
