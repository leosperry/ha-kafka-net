namespace HaKafkaNet;

public interface IAutomation
{
    /// <summary>
    /// At startup, depending on the specifics of when events fired, timing of events relative to the cache can vary
    /// Use this setting to tell HaKafkaNet which events you care about
    /// </summary>
    virtual EventTiming EventTiming {get => EventTiming.PostStartup; }

    /// <summary>
    /// When a state change occurs, if the entity id of the state matches any value (case sensitive) in this collection,
    /// the execute method will be called
    /// </summary>
    /// <returns>entity id's which your automation should be notified about</returns>
    IEnumerable<string> TriggerEntityIds();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateChange">Information about the state change including the previous state if it was cached</param>
    /// <param name="cancellationToken">A token that will be marked canceled during shutdown of the worker</param>
    /// <returns></returns>
    Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken);
}
