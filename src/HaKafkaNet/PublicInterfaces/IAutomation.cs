using System.Text.Json;

namespace HaKafkaNet;

/// <summary>
/// Common properties/methods for all automation types
/// </summary>
public interface IAutomationBase
{
    /// <summary>
    /// When true, the framework will actively get the state of an entity and trigger your automation 
    /// at least once at startup. For delayable automations, the framework will do the same after delayed execution
    /// see: https://github.com/leosperry/ha-kafka-net/wiki/Passive-vs-Active-automations
    /// </summary>
    public bool IsActive { get => false; }

    /// <summary>
    /// At startup, depending on the specifics of when events fired, timing of events relative to the cache can vary
    /// Use this setting to tell HaKafkaNet which events you care about
    /// </summary>
    EventTiming EventTimings { get => EventTiming.PostStartup; }

    /// <summary>
    /// When a state change occurs, if the entity id of the state matches any value (case sensitive) in this collection,
    /// the ContinuesToBeTrue method will be called
    /// </summary>
    /// <returns>entity id's which your automation should be notified about</returns>
    IEnumerable<string> TriggerEntityIds();
}

/// <summary>
/// A strongly typed simple automation
/// </summary>
/// <typeparam name="Tchange"></typeparam>
/// <typeparam name="Tentity"></typeparam>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface IAutomation<in Tchange, Tentity, Tstate, Tatt> : IAutomationBase
    where Tchange : HaEntityStateChange<Tentity>
    where Tentity : HaEntityState<Tstate, Tatt>
{
    /// <summary>
    /// Action to be taken when state changes
    /// </summary>
    /// <param name="stateChange"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Execute(Tchange stateChange, CancellationToken ct);
}

/// <summary>
/// Defines methods for strongly typed automations
/// </summary>
public interface IFallbackExecution
{
    /// <summary>
    /// Called when type conversion fails during state change
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="stateChange"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task FallbackExecute(Exception ex, HaEntityStateChange stateChange, CancellationToken ct);
}

/// <summary>
/// strongly typed simple automation
/// </summary>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface IAutomation<Tstate, Tatt> 
    : IAutomationBase, IAutomation<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, HaEntityState<Tstate, Tatt> ,Tstate, Tatt>;

/// <summary>
/// strongly typed simple automation
/// </summary>
/// <typeparam name="Tstate"></typeparam>
public interface IAutomation<Tstate> : IAutomation<Tstate, JsonElement>;

/// <summary>
/// A simple automation
/// </summary>
public interface IAutomation : IAutomationBase, IAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>;

/// <summary>
/// common properties/methods for Conditional and Schedulable automations
/// </summary>
public interface IDelayableAutomationBase
{
    /// <summary>
    /// In some cases, especially if handling pre-startup events, this determines if executions should run if it was scheduled to run prior to startup
    /// </summary>
    bool ShouldExecutePastEvents { get => false; }

    /// <summary>
    /// Indicates if the automation should run if the ContinuesToBeTrue method errors
    /// </summary>
    bool ShouldExecuteOnContinueError { get => false; }

    /// <summary>
    /// Called after the first time ContinuesToBeTrue is called and after the For specified amount of time
    /// as long as all the calls to ContinuesToBeTrue returned true during that time
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Execute(CancellationToken ct);
}

/// <summary>
/// strongly typed delayable
/// </summary>
/// <typeparam name="Tchange"></typeparam>
/// <typeparam name="Tentity"></typeparam>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface IDelayableAutomation<in Tchange, Tentity, Tstate, Tatt> : IAutomationBase, IDelayableAutomationBase
    where Tchange : HaEntityStateChange<Tentity>
    where Tentity : HaEntityState<Tstate, Tatt>
{
    /// <summary>
    /// When this method returns true, execution will be scheduled
    /// </summary>
    /// <param name="stateChange"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> ContinuesToBeTrue(Tchange stateChange, CancellationToken ct);
}

/// <summary>
/// strongly typed delayable
/// </summary>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface IDelayableAutomation<Tstate, Tatt> : 
    IDelayableAutomation<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, HaEntityState<Tstate, Tatt> ,Tstate, Tatt>;

/// <summary>
/// strongly typed delayable
/// </summary>
public interface IDelayableAutomation : IDelayableAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>;

/// <summary>
/// Common properties for both generic and non-generic conditional automations
/// </summary>
public interface IConditionalAutomationBase
{
    /// <summary>
    /// If ContinuesToBeTrue returns true, this is used to determine when Execute should be called.
    /// If this returns TimeSpan.MinValue, your automation will not execute as if ContinuesToBeTrue returned false
    /// </summary>
    /// <returns></returns>
    TimeSpan For{ get; }
}

/// <summary>
/// strongly typed conditional automation
/// </summary>
/// <typeparam name="Tchange"></typeparam>
/// <typeparam name="Tentity"></typeparam>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface IConditionalAutomation<in Tchange, Tentity, Tstate, Tatt> : IDelayableAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>, IConditionalAutomationBase
{

}

/// <summary>
/// strongly typed conditional automation
/// </summary>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface IConditionalAutomation<Tstate, Tatt> : IDelayableAutomation<Tstate, Tatt>, IConditionalAutomationBase;

/// <summary>
/// strongly typed conditional automation
/// </summary>
/// <typeparam name="Tstate"></typeparam>
public interface IConditionalAutomation<Tstate> : IDelayableAutomation<Tstate, JsonElement>, IConditionalAutomationBase;

/// <summary>
/// strongly typed conditional automation
/// </summary>
public interface IConditionalAutomation : IDelayableAutomation, IConditionalAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>;

/// <summary>
/// common properties and methods for generic and non-generic schedulable automations
/// </summary>
public interface ISchedulableAutomationBase
{
    /// <summary>
    /// True if new state changes can override previously scheduled tasks
    /// </summary>
    bool IsReschedulable { get; }

    /// <summary>
    /// If ContinuesToBeTrue returns true, this is used to calculate when Execute should be called
    /// If this returns null, your automation will not execute as if ContinuesToBeTrue returned false. 
    /// </summary>
    /// <returns></returns>
    DateTimeOffset? GetNextScheduled();
}

/// <summary>
/// strongly typed schedulable automation
/// </summary>
/// <typeparam name="Tchange"></typeparam>
/// <typeparam name="Tentity"></typeparam>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface ISchedulableAutomation<in Tchange, Tentity, Tstate, Tatt> : IDelayableAutomation<Tchange, Tentity, Tstate, Tatt> , ISchedulableAutomationBase   
    where Tchange : HaEntityStateChange<Tentity>
    where Tentity : HaEntityState<Tstate, Tatt>;

/// <summary>
/// strongly typed schedulable automation
/// </summary>
/// <typeparam name="Tstate"></typeparam>
/// <typeparam name="Tatt"></typeparam>
public interface ISchedulableAutomation<Tstate, Tatt> : IDelayableAutomation<Tstate,Tatt>, ISchedulableAutomationBase;

/// <summary>
/// strongly typed schedulable automation
/// </summary>
/// <typeparam name="Tstate"></typeparam>
public interface ISchedulableAutomation<Tstate> : IDelayableAutomation<Tstate, JsonElement>, ISchedulableAutomationBase;

/// <summary>
/// strongly typed schedulable automation
/// </summary>
public interface ISchedulableAutomation : IDelayableAutomation, ISchedulableAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>;

