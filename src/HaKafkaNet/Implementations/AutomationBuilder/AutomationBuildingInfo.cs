namespace HaKafkaNet;

public abstract class AutomationBuildingInfo
{
    internal Guid? Id { get; set; }
    internal string? Name { get; set; }
    internal string? Description { get; set; }
    internal bool EnabledAtStartup { get; set; }
    internal IEnumerable<string>? TriggerEntityIds { get; set; }
    internal IEnumerable<string>? AdditionalEntitiesToTrack { get; set; }
}

public abstract class SimpleAutomationBuildignInfoBase: AutomationBuildingInfo
{
    internal EventTiming? EventTimings { get; set; }
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

public abstract class ConditionalAutomationBuildingInfoBase : AutomationBuildingInfo
{
    internal TimeSpan? For { get; set; }
}

public class ConditionalAutomationBuildingInfo : ConditionalAutomationBuildingInfoBase
{
    internal Func<HaEntityStateChange, CancellationToken, Task<bool>>? ContinuesToBeTrue { get; set; }

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
