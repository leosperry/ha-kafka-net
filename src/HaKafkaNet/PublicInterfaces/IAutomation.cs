﻿using System.Text.Json;

namespace HaKafkaNet;

public interface IInitializeOnStartup
{
    Task Initialize();
}

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

public interface IAutomation<in Tchange, Tentity, Tstate, Tatt> 
    where Tchange : HaEntityStateChange<Tentity>
    where Tentity : HaEntityState<Tstate, Tatt>
{
    Task Execute(Tchange stateChange, CancellationToken ct);
}

public interface IAutomation<Tstate, Tatt> : IAutomationBase, IAutomation<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, HaEntityState<Tstate, Tatt> ,Tstate, Tatt>
{

}

public interface IAutomation<Tstate> : IAutomation<Tstate, JsonElement>;

public interface IAutomation : IAutomationBase, IAutomation<HaEntityStateChange, HaEntityState, string, JsonElement>
{
    /// <summary>
    /// The interface used by the Automation manager to execute automations
    /// </summary>
    /// <param name="stateChange">Information about the state change including the previous state if it was cached</param>
    /// <param name="cancellationToken">A token that will be marked canceled during shutdown of the worker</param>
    /// <returns></returns>
    //Task Execute(HaEntityStateChange stateChange, CancellationToken ct);
}

/// <summary>
/// used internally
/// consider moving to another file
/// </summary>
internal interface IAutomationWrapper<out T> : IAutomation, IInitializeOnStartup, IAutomationMeta where T : notnull
{
    T WrappedAutomation { get; }
    Task IInitializeOnStartup.Initialize() => (WrappedAutomation as IInitializeOnStartup)?.Initialize() ?? Task.CompletedTask;
    AutomationMetaData IAutomationMeta.GetMetaData() => (WrappedAutomation as IAutomationMeta)?.GetMetaData() ?? AutomationMetaData.Create(this.WrappedAutomation);
}

public interface IDelayableAutomation : IAutomationBase
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
    /// Called on all state changes where EntityId matches any of the TriggerEntityIds
    /// </summary>
    /// <param name="haEntityStateChange"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>boolean to indicate if the automation should be schecduled or cancled if scheduled</returns>
    Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken ct);

    /// <summary>
    /// Called after the first time ContinuesToBeTrue is called and after the For specified amount of time
    /// as long as all the calls to ContinuesToBeTrue returned true during that time
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Execute(CancellationToken ct);
}

public interface IConditionalAutomation : IDelayableAutomation
{
    /// <summary>
    /// If ContinuesToBeTrue returns true, this is used to determine when Execute should be called.
    /// If this returns TimeSpan.MinValue, your automation will not execute as if ContinuesToBeTrue returned false
    /// </summary>
    /// <returns></returns>
    TimeSpan For{ get; }
}

public interface ISchedulableAutomation : IDelayableAutomation
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

public interface IAutomationMeta
{
    AutomationMetaData GetMetaData();
}

public interface ISetAutomationMeta
{
    void SetMeta(AutomationMetaData meta);
}
