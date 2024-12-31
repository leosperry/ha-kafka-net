using Confluent.Kafka;
using Microsoft.AspNetCore.Identity.Data;

namespace HaKafkaNet;

public partial class AutomationBuilderExtensions
{
    /// <summary>
    /// When ContinuesToBeTrue throws, instructs the automation to continue if already scheduled
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <returns></returns>
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

    /// <summary>
    /// called for all state changes
    /// </summary>
    /// <param name="info"></param>
    /// <param name="continuesToBeTrue"></param>
    /// <returns></returns>
    public static ConditionalAutomationBuildingInfo When(
        this ConditionalAutomationBuildingInfo info,
        Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue)
    {
        info.ContinuesToBeTrue = continuesToBeTrue;
        return info;
    }

    /// <summary>
    /// called for all state changes
    /// </summary>
    /// <param name="info"></param>
    /// <param name="continuesToBeTrue"></param>
    /// <returns></returns>
    public static ConditionalAutomationBuildingInfo When(
        this ConditionalAutomationBuildingInfo info,
        Func<HaEntityStateChange, bool> continuesToBeTrue)
    {
        info.ContinuesToBeTrue = (st, _) => Task.FromResult(continuesToBeTrue(st));
        return info;
    }

    /// <summary>
    /// called for all state changes
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="info"></param>
    /// <param name="continuesToBeTrue"></param>
    /// <returns></returns>
    public static TypedConditionalBuildingInfo<Tstate, Tatt> When<Tstate, Tatt>(
        this TypedConditionalBuildingInfo<Tstate, Tatt> info,
        Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task<bool>> continuesToBeTrue
        )
    {
        info.ContinuesToBeTrue = continuesToBeTrue;
        return info;
    }

    /// <summary>
    /// called for all state changes
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="info"></param>
    /// <param name="continuesToBeTrue"></param>
    /// <returns></returns>
    public static TypedConditionalBuildingInfo<Tstate, Tatt> When<Tstate, Tatt>(
        this TypedConditionalBuildingInfo<Tstate, Tatt> info,
        Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, bool> continuesToBeTrue
        )
    {
        info.ContinuesToBeTrue = (sc, ct) => Task.FromResult(continuesToBeTrue(sc));
        return info;
    }

    /// <summary>
    /// The amount of time to wait after the first time When returns true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="for"></param>
    /// <returns></returns>
    public static T For<T>(this T info, TimeSpan @for) where T : DelayableAutomationBuildingInfo
    {
        info.For = @for;
        return info;
    }

    /// <summary>
    /// The number of seconds to wait after the first time When returns true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static T ForSeconds<T>(this T info, int seconds) where T : DelayableAutomationBuildingInfo
    {
        info.For = TimeSpan.FromSeconds(seconds);
        return info;
    }

    /// <summary>
    /// The number of minutes to wait after the first time When returns true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="minutes"></param>
    /// <returns></returns>
    public static T ForMinutes<T>(this T info, int minutes) where T : DelayableAutomationBuildingInfo
    {
        info.For = TimeSpan.FromMinutes(minutes);
        return info;
    }

    /// <summary>
    /// The number of hours to wait after the first time When returns true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="hours"></param>
    /// <returns></returns>
    public static T ForHours<T>(this T info, int hours) where T : DelayableAutomationBuildingInfo
    {
        info.For = TimeSpan.FromHours(hours);
        return info;
    }

    /// <summary>
    /// the action to execute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="executor"></param>
    /// <returns></returns>
    public static T Then<T>(this T info, Func<CancellationToken, Task> executor)
        where T : ConditionalAutomationBuildingInfoBase
    {
        info.Execution = executor;
        return info;
    }

    /// <summary>
    /// Tells the framework that this automation can be rescheduled based on new state changes
    /// automations default to false
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="isReschedulable"></param>
    /// <returns></returns>
    public static T SetReschedulable<T>(this T info,  bool isReschedulable = true)
        where T : SchedulableAutomationBuildingInfoBase
    {
        info.IsReschedulable = isReschedulable;
        return info;
    }

    /// <summary>
    /// Tells the automation how to get the next scheduled time
    /// </summary>
    /// <param name="info"></param>
    /// <param name="getNextFromState">Asynchronous method that should return a DateTime? based on input state change. Return null if you do not want to schedule</param>
    /// <returns></returns>
    public static SchedulableAutomationBuildingInfo GetNextScheduled(this SchedulableAutomationBuildingInfo info, GetNextEventFromEntityState getNextFromState)
    {
        info.GetNextScheduled = getNextFromState;
        return info;
    }

    /// <summary>
    /// Tells the automation how to get the next scheduled time
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="info"></param>
    /// <param name="getNextFromState"></param>
    /// <returns></returns>
    public static TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> GetNextScheduled<Tstate, Tatt>(
        this TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> info, GetNextEventFromEntityState<Tstate, Tatt> getNextFromState)
    {
        info.GetNextScheduledInternal = getNextFromState;
        return info;
    }

    /// <summary>
    /// same as When for conditional automations
    /// </summary>
    /// <param name="info"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static SchedulableAutomationBuildingInfo While(this SchedulableAutomationBuildingInfo info, Func<HaEntityStateChange ,bool> condition)
    {
        info.WhileCondition = condition;
        return info;
    }

    /// <summary>
    /// the action to execute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public static T WithExecution<T>(this T info, Func<CancellationToken, Task> execution)
        where T : DelayableAutomationBuildingInfo
    {
        info.Execution = execution;
        return info;
    }

    /// <summary>
    /// same as When for conditional automations
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="info"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> While<Tstate, Tatt>(
        this TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> info, Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, bool> condition)
    {
        info.WhileCondition = condition;
        return info;
    }
}
