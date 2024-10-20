﻿namespace HaKafkaNet;

public abstract class AutomationBuildingInfo
{
    internal string? KeyRequest { get; set; }
    internal string? Name { get; set; }
    internal string? Description { get; set; }
    internal bool EnabledAtStartup { get; set; }
    internal EventTiming? EventTimings { get; set; }
    internal bool TriggerOnBadState { get; set; } = false;
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

public class SchedulableAutomationBuildingInfo : DelayableAutomationBuildingInfo
{
    internal bool IsReschedulable { get; set; }

    internal Func<CancellationToken, Task>? Execution { get; set; }
    internal GetNextEventFromEntityState? GetNextScheduled { get; set; }
    internal Func<HaEntityStateChange, bool>? WhileCondition { get; set; }
    internal TimeSpan? ForTime { get; set; }
}

public class SunAutommationBuildingInfo : AutomationBuildingInfo
{
    internal bool ExecutePast { get; set; } = true;
    internal SunEventType SunEvent { get; set; }
    internal TimeSpan? Offset { get; set; }
    internal Func<CancellationToken, Task>? Execution { get; set; }
}
