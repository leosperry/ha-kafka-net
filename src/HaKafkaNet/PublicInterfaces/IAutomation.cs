using System.Text.Json;

namespace HaKafkaNet;

public interface IAutomationBase
{
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

public interface IAutomation<in Tchange, Tentity, Tstate, Tatt> : IAutomationBase
    where Tchange : HaEntityStateChange<Tentity>
    where Tentity : HaEntityState<Tstate, Tatt>
{
    Task Execute(Tchange stateChange, CancellationToken ct);
}

public interface IAutomation<Tstate, Tatt> 
    : IAutomationBase, IAutomation<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, HaEntityState<Tstate, Tatt> ,Tstate, Tatt>;

public interface IAutomation<Tstate> : IAutomation<Tstate, JsonElement>;
public interface IAutomation : IAutomationBase, IAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>;

public interface IDelayableAutomationBase
{
    /// <summary>
    /// In some cases, especially if handling prestartup events, this determines if executions should run if it was scheduled to run previous to DateTime.Now
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Execute(CancellationToken ct);
}
public interface IDelayableAutomation<in Tchange, Tentity, Tstate, Tatt> : IAutomationBase, IDelayableAutomationBase
    where Tchange : HaEntityStateChange<Tentity>
    where Tentity : HaEntityState<Tstate, Tatt>
{
    Task<bool> ContinuesToBeTrue(Tchange stateChange, CancellationToken ct);
}

public interface IDelayableAutomation<Tstate, Tatt> : 
    IDelayableAutomation<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, HaEntityState<Tstate, Tatt> ,Tstate, Tatt>;
// public interface IDelayableAutomation<Tstate> :
//     IDelayableAutomation<HaEntityStateChange<HaEntityState<Tstate, JsonElement>>, HaEntityState<Tstate, JsonElement> ,Tstate, JsonElement>;
public interface IDelayableAutomation : IDelayableAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>;




public interface IConditionalAutomationBase
{
    /// <summary>
    /// If ContinuesToBeTrue returns true, this is used to determine when Execute should be called.
    /// If this returns TimeSpan.MinValue, your automation will not execute as if ContinuesToBeTrue returned false
    /// </summary>
    /// <returns></returns>
    TimeSpan For{ get; }
}
public interface IConditionalAutomation<in Tchange, Tentity, Tstate, Tatt> : IDelayableAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>, IConditionalAutomationBase
{

}

public interface IConditionalAutomation<Tstate, Tatt> : IDelayableAutomation<Tstate, Tatt>, IConditionalAutomationBase;
public interface IConditionalAutomation<Tstate> : IDelayableAutomation<Tstate, JsonElement>, IConditionalAutomationBase;
public interface IConditionalAutomation : IDelayableAutomation, IConditionalAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>;


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
    DateTime? GetNextScheduled();
}

public interface ISchedulableAutomation<in Tchange, Tentity, Tstate, Tatt> : IDelayableAutomation<Tchange, Tentity, Tstate, Tatt> , ISchedulableAutomationBase   
    where Tchange : HaEntityStateChange<Tentity>
    where Tentity : HaEntityState<Tstate, Tatt>;

public interface ISchedulableAutomation<Tstate, Tatt> : IDelayableAutomation<Tstate,Tatt>, ISchedulableAutomationBase;
public interface ISchedulableAutomation<Tstate> : IDelayableAutomation<Tstate, JsonElement>, ISchedulableAutomationBase;
public interface ISchedulableAutomation : IDelayableAutomation, ISchedulableAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>;

