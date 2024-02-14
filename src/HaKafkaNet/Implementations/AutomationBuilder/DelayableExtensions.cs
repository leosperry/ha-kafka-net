namespace HaKafkaNet;

public partial class AutomationBuilderExtensions
{
    public static T ShouldContinueOnError<T>(this T info) where T : DelayableAutomationBuildingInfo
    {
        info.ShouldExecuteOnContinueError = true;
        return info;
    }

    public static T ShouldExecutePastEvents<T>(this T info) where T : DelayableAutomationBuildingInfo
    {
        info.ShouldExecutePastEvents = true;
        return info;
    }

    public static ConditionalAutomationWithServicesBuildingInfo When(
        this ConditionalAutomationWithServicesBuildingInfo info,
        Func<IHaServices, HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue)
    {
        info.ContinuesToBeTrueWithServices = continuesToBeTrue;
        return info;
    }

    public static ConditionalAutomationWithServicesBuildingInfo When(
        this ConditionalAutomationWithServicesBuildingInfo info,
        Func<HaEntityStateChange, bool> continuesToBeTrue)
    {
        info.ContinuesToBeTrueWithServices = (_, sc, _) => Task.FromResult( continuesToBeTrue(sc));
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

    public static T For<T>(this T info, TimeSpan @for) where T : ConditionalAutomationBuildingInfoBase
    {
        info.For = @for;
        return info;
    }

    public static T ForSeconds<T>(this T info, int seconds) where T : ConditionalAutomationBuildingInfoBase
    {
        info.For = TimeSpan.FromSeconds(seconds);
        return info;
    }

    public static T ForMinutes<T>(this T info, int minutes) where T : ConditionalAutomationBuildingInfoBase
    {
        info.For = TimeSpan.FromMinutes(minutes);
        return info;
    }

    public static T ForHours<T>(this T info, int hours) where T : ConditionalAutomationBuildingInfoBase
    {
        info.For = TimeSpan.FromHours(hours);
        return info;
    }

    public static ConditionalAutomationBuildingInfo Then(this ConditionalAutomationBuildingInfo info, Func<CancellationToken, Task> executor)
    {
        info.Execution = executor;
        return info;
    }

    public static ConditionalAutomationWithServicesBuildingInfo Then(this ConditionalAutomationWithServicesBuildingInfo info, 
    Func<IHaServices, CancellationToken ,Task> executor)
    {
        info.ExecutionWithServices = executor;
        return info;
    }

    public static SchedulableAutomationBuildingInfo SetReschedulable(this SchedulableAutomationBuildingInfo info,  bool isReschedulable)
    {
        info.IsReschedulable = isReschedulable;
        return info;
    }

    public static SchedulableAutomationBuildingInfo GetNextScheduled(this SchedulableAutomationBuildingInfo info, GetNextEventFromEntityState getNextFromState)
    {
        info.GetNextScheduled = getNextFromState;
        return info;
    }

    public static SchedulableAutomationBuildingInfo WithExecution(this SchedulableAutomationBuildingInfo info, Func<CancellationToken, Task> execution)
    {
        info.Execution = execution;
        return info;
    }
}
