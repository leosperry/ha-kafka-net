namespace HaKafkaNet;

public abstract class AutomationBuildingInfo
{
    internal string? KeyRequest { get; set; }
    internal string? Name { get; set; }
    internal string? Description { get; set; }
    internal bool EnabledAtStartup { get; set; }
    internal EventTiming? EventTimings { get; set; }
    internal bool TriggerOnBadState { get; set; } = false;
    internal AutomationMode Mode { get; set; }
}

public abstract class MostAutomationsBuildingInfo : AutomationBuildingInfo
{
    internal IEnumerable<string>? TriggerEntityIds { get; set; }
    internal IEnumerable<string>? AdditionalEntitiesToTrack { get; set; }
}

public class SimpleAutomationBuildingInfo : MostAutomationsBuildingInfo
{
    internal Func<HaEntityStateChange, CancellationToken, Task>? Execution { get; set; }
}

public class TypedAutomationBuildingInfo<Tstate, Tatt> : MostAutomationsBuildingInfo
{
    internal Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task>? Execution { get; set; }
}

public abstract class DelayableAutomationBuildingInfo : MostAutomationsBuildingInfo
{
    internal bool ShouldExecutePastEvents { get; set; }
    internal bool ShouldExecuteOnContinueError { get; set; }
    internal Func<CancellationToken, Task>? Execution { get; set; }
    internal TimeSpan? For { get; set; }

}

public abstract class ConditionalAutomationBuildingInfoBase : DelayableAutomationBuildingInfo
{
}

public class ConditionalAutomationBuildingInfo : ConditionalAutomationBuildingInfoBase
{
    internal Func<HaEntityStateChange, CancellationToken, Task<bool>>? ContinuesToBeTrue { get; set; }
}

public class TypedConditionalBuildingInfo<Tstate, Tatt> : ConditionalAutomationBuildingInfoBase
{
    internal Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task<bool>>? ContinuesToBeTrue { get; set; }
}

public abstract class SchedulableAutomationBuildingInfoBase : DelayableAutomationBuildingInfo
{
    internal bool IsReschedulable { get; set; }
}

public class SchedulableAutomationBuildingInfo : SchedulableAutomationBuildingInfoBase
{
    internal GetNextEventFromEntityState? GetNextScheduled { get; set; }

    internal Func<HaEntityStateChange, bool>? WhileCondition { get; set; }
}

public class TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> : SchedulableAutomationBuildingInfoBase
{
    internal GetNextEventFromEntityState<Tstate, Tatt>? GetNextScheduled { get; set; }

    internal Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, bool>? WhileCondition { get; set; }
}



public class SunAutomationBuildingInfo : AutomationBuildingInfo
{
    internal bool ExecutePast { get; set; } = true;
    internal SunEventType SunEvent { get; set; }
    internal TimeSpan? Offset { get; set; }
    internal Func<CancellationToken, Task>? Execution { get; set; }
}
