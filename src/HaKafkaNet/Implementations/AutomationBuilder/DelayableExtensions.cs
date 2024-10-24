using Confluent.Kafka;
using Microsoft.AspNetCore.Identity.Data;

namespace HaKafkaNet;

public partial class AutomationBuilderExtensions
{
    public static T ShouldContinueOnError<T>(this T info) where T : DelayableAutomationBuildingInfo
    {
        info.ShouldExecuteOnContinueError = true;
        return info;
    }

    /// <summary>
    /// Tells the builder that automation should execute past events. It does not by default.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <returns></returns>
    public static T ShouldExecutePastEvents<T>(this T info) where T : DelayableAutomationBuildingInfo
    {
        info.ShouldExecutePastEvents = true;
        return info;
    }

    /// <summary>
    /// Creates an automation that will survive restarts.
    /// Sets EventTimings to Durable
    /// Sets both ShouldExecutePastEvents and IsReschedulable to true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <returns></returns>
    public static T MakeDurable<T>(this T info) where T : SchedulableAutomationBuildingInfoBase
    {
        info.ShouldExecutePastEvents = true;
        info.EventTimings = EventTiming.Durable;

        return info;
    }

    public static ConditionalAutomationBuildingInfo When(
        this ConditionalAutomationBuildingInfo info,
        Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue)
    {
        info.ContinuesToBeTrue = continuesToBeTrue;
        return info;
    }

    public static ConditionalAutomationBuildingInfo When(
        this ConditionalAutomationBuildingInfo info,
        Func<HaEntityStateChange, bool> continuesToBeTrue)
    {
        info.ContinuesToBeTrue = (st, _) => Task.FromResult(continuesToBeTrue(st));
        return info;
    }

    public static TypedConditionalBuildingInfo<Tstate, Tatt> When<Tstate, Tatt>(
        this TypedConditionalBuildingInfo<Tstate, Tatt> info,
        Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task<bool>> continuesToBeTrue
        )
    {
        info.ContinuesToBeTrue = continuesToBeTrue;
        return info;
    }

    public static TypedConditionalBuildingInfo<Tstate, Tatt> When<Tstate, Tatt>(
        this TypedConditionalBuildingInfo<Tstate, Tatt> info,
        Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, bool> continuesToBeTrue
        )
    {
        info.ContinuesToBeTrue = (sc, ct) => Task.FromResult(continuesToBeTrue(sc));
        return info;
    }

    public static T For<T>(this T info, TimeSpan @for) where T : DelayableAutomationBuildingInfo
    {
        info.For = @for;
        return info;
    }

    public static T ForSeconds<T>(this T info, int seconds) where T : DelayableAutomationBuildingInfo
    {
        info.For = TimeSpan.FromSeconds(seconds);
        return info;
    }

    public static T ForMinutes<T>(this T info, int minutes) where T : DelayableAutomationBuildingInfo
    {
        info.For = TimeSpan.FromMinutes(minutes);
        return info;
    }

    public static T ForHours<T>(this T info, int hours) where T : DelayableAutomationBuildingInfo
    {
        info.For = TimeSpan.FromHours(hours);
        return info;
    }

    public static T Then<T>(this T info, Func<CancellationToken, Task> executor)
        where T : ConditionalAutomationBuildingInfoBase
    {
        info.Execution = executor;
        return info;
    }

    public static SchedulableAutomationBuildingInfo SetReschedulable(this SchedulableAutomationBuildingInfo info,  bool isReschedulable)
    {
        info.IsReschedulable = isReschedulable;
        return info;
    }

    /// <summary>
    /// Tells the automation how to get the next scheuled time
    /// </summary>
    /// <param name="info"></param>
    /// <param name="getNextFromState">Asynchronous method that should return a DateTime? based on input state change. Return null if you do not want to schedule</param>
    /// <returns></returns>
    public static SchedulableAutomationBuildingInfo GetNextScheduled(this SchedulableAutomationBuildingInfo info, GetNextEventFromEntityState getNextFromState)
    {
        info.GetNextScheduled = getNextFromState;
        return info;
    }

    public static TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> GetNextScheduled<Tstate, Tatt>(
        this TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> info, GetNextEventFromEntityState<Tstate, Tatt> getNextFromState)
    {
        info.GetNextScheduled = getNextFromState;
        return info;
    }

    public static SchedulableAutomationBuildingInfo While(this SchedulableAutomationBuildingInfo info, Func<HaEntityStateChange ,bool> condition)
    {
        info.WhileCondition = condition;
        return info;
    }

    public static T WithExecution<T>(this T info, Func<CancellationToken, Task> execution)
        where T : DelayableAutomationBuildingInfo
    {
        info.Execution = execution;
        return info;
    }

    public static TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> While<Tstate, Tatt>(
        this TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> info, Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, bool> condition)
    {
        info.WhileCondition = condition;
        return info;
    }
}
