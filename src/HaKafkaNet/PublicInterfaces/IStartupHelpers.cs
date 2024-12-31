namespace HaKafkaNet;


/// <summary>
/// Collection of services best used at startup 
/// </summary>
public interface IStartupHelpers
{
    /// <summary>
    /// provides methods for quickly building automations
    /// </summary>
    public IAutomationBuilder Builder { get; }

    /// <summary>
    /// a small number of prebuilt automations
    /// </summary>
    public IAutomationFactory Factory { get; }

    /// <summary>
    /// provides entities that automatically updates as their state changes
    /// best used for things that don't update often and/or when millisecond timing is not critical
    /// see: https://github.com/leosperry/ha-kafka-net/wiki/Updating-Entity-Provider
    /// </summary>
    public IUpdatingEntityProvider UpdatingEntityProvider { get; }
}
