namespace HaKafkaNet;

public abstract class AutomationBuildingInfo
{
    internal Guid? Id { get; set; }
    internal string? Name { get; set; }
    internal string? Description { get; set; }
    internal bool EnabledAtStartup { get; set; }
    internal IEnumerable<string>? TriggerEntityIds { get; set; }
    internal IEnumerable<string>? AdditionalEntitiesToTrack { get; set; }
    internal EventTiming? EventTimings { get; set; }
}

public abstract class SimpleAutomationBuildignInfoBase: AutomationBuildingInfo
{
    
}

public class SimpleAutomationBuildingInfo : SimpleAutomationBuildignInfoBase
{
    internal Func<HaEntityStateChange, CancellationToken, Task>? Execution { get; set; }
}

public class SimpleAutomationWithServicesBuildingInfo : SimpleAutomationBuildignInfoBase
{
    internal readonly IHaServices _services;

    internal SimpleAutomationWithServicesBuildingInfo(IHaServices services)
    {
        _services = services;
    }
    internal Func<IHaServices, HaEntityStateChange, CancellationToken, Task>? ExecutionWithServcies { get; set; }
}

public abstract class DelayableAutomationBuildingInfo : AutomationBuildingInfo
{
    internal bool ShouldExecutePastEvents { get; set; }
    internal bool ShouldExecuteOnContinueError { get; set; }
}

public abstract class ConditionalAutomationBuildingInfoBase : DelayableAutomationBuildingInfo
{
    internal TimeSpan? For { get; set; }
    internal Func<HaEntityStateChange, CancellationToken, Task<bool>>? ContinuesToBeTrue { get; set; }
}

public class ConditionalAutomationBuildingInfo : ConditionalAutomationBuildingInfoBase
{
    internal Func<CancellationToken, Task>? Execution { get; set; }
}

public class ConditionalAutomationWithServicesBuildingInfo : ConditionalAutomationBuildingInfoBase
{
    internal readonly IHaServices _services;
    internal ConditionalAutomationWithServicesBuildingInfo(IHaServices services)
    {
        _services = services!;
    }
    internal Func<IHaServices ,HaEntityStateChange, CancellationToken, Task<bool>>? ContinuesToBeTrueWithServices { get; set; }
    internal Func<IHaServices, CancellationToken, Task>? ExecutionWithServices { get; set; }
}

public abstract class SchedulableAutomationBuildingInfoBase : DelayableAutomationBuildingInfo
{
        internal bool IsReschedulable { get; set; }
}

public class SchedulableAutomationBuildingInfo : SchedulableAutomationBuildingInfoBase
{
    internal Func<CancellationToken, Task>? Execution { get; set; }
    internal GetNextEventFromEntityState? GetNextScheduled { get; set; }
}

public class SchedulableAutomationWithServicesBuildingInfo : SchedulableAutomationBuildingInfoBase
{
    internal readonly IHaServices _services;

    internal SchedulableAutomationWithServicesBuildingInfo(IHaServices services)
    {
        _services = services;
    }

    internal Func<IHaServices ,HaEntityStateChange, CancellationToken, Task<DateTime?>>? GetNextScheduledWithServices { get; set; }
    internal Func<IHaServices, CancellationToken, Task>? ExecutionWithServices { get; set; }
    
}