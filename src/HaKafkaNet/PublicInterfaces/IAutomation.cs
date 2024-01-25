namespace HaKafkaNet;

public interface IAutomation
{
    /// <summary>
    /// User friendly name to display
    /// </summary>
    string Name { get => GetType().Name; }

    /// <summary>
    /// At startup, depending on the specifics of when events fired, timing of events relative to the cache can vary
    /// Use this setting to tell HaKafkaNet which events you care about
    /// </summary>
    EventTiming EventTimings { get => EventTiming.PostStartup; }

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

public interface IConditionalAutomation
{
    /// <summary>
    /// User friendly name to display
    /// </summary>
    string Name { get => GetType().Name; }

    /// <summary>
    /// When a state change occurs, if the entity id of the state matches any value (case sensitive) in this collection,
    /// the ContinuesToBeTrue method will be called
    /// </summary>
    /// <returns>entity id's which your automation should be notified about</returns>
    IEnumerable<string> TriggerEntityIds();

    /// <summary>
    /// A positive duration to wait. If ContinuesToBeTrue has never returned false, your automation 
    /// will be executed after the first time it reports true and this time has elapsed
    /// </summary>
    /// <returns></returns>
    TimeSpan For{ get; }

    /// <summary>
    /// Called on all state changes where EntityId matches any of the TriggerEntityIds
    /// </summary>
    /// <param name="haEntityStateChange"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken);

    /// <summary>
    /// Called after the first time ContinuesToBeTrue is called and after the For specified amount of time
    /// as long as all the calls to ContinuesToBeTrue returned true during that time
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Execute(CancellationToken cancellationToken);
}
