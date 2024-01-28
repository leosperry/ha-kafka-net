namespace HaKafkaNet;
public static class AutomationBuilderExtensions
{
    public static T WithId<T>(this T info, Guid id) where T: AutomationBuildingInfo
    {
        info.Id = id;
        return info;
    }

    public static T WithName<T>(this T info, string name) where T: AutomationBuildingInfo
    {
        info.Name = name;
        return info;
    }

    public static T WithDescription<T>(this T info, string description) where T: AutomationBuildingInfo
    {
        info.Description = description;
        return info;
    }

    public static T WithTriggers<T>(this T info, params string[] triggerEntityIds) where T: AutomationBuildingInfo
    {
        info.TriggerEntityIds = triggerEntityIds;
        return info;
    }

    public static SimpleAutomationBuildingInfo WithExecution(this SimpleAutomationBuildingInfo info, Func<HaEntityStateChange, CancellationToken, Task> execution)
    {
        info.Execution = execution;
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

    public static ConditionalAutomationBuildingInfo For(this ConditionalAutomationBuildingInfo info, TimeSpan @for)
    {
        info.For = @for;
        return info;
    }

    public static ConditionalAutomationBuildingInfo ForSeconds(this ConditionalAutomationBuildingInfo info, int seconds)
    {
        info.For = TimeSpan.FromSeconds(seconds);
        return info;
    }

    public static ConditionalAutomationBuildingInfo ForMinutes(this ConditionalAutomationBuildingInfo info, int minutes)
    {
        info.For = TimeSpan.FromMinutes(minutes);
        return info;
    }

    public static ConditionalAutomationBuildingInfo ForHours(this ConditionalAutomationBuildingInfo info, int hours)
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

    public static IAutomation Build(this SimpleAutomationBuildingInfo info)
    {
        return new SimpleAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.Execution ?? throw new AutomationBuilderException("execution must be defined"),
            info.EventTimings ?? EventTiming.PostStartup).WithMeta(GetMeta(info));

    }
    
    public static IAutomation Build(this SimpleAutomationWithServicesBuildingInfo info)
    {
        return new SimpleAutomationWithServices(
            info._services,
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.ExecutionWithServcies ?? throw new AutomationBuilderException("execution must be defined"),
            info.EventTimings ?? EventTiming.PostStartup).WithMeta(GetMeta(info));
    }

    public static IConditionalAutomation Build(this ConditionalAutomationBuildingInfo info)
    {
        return new ConditionalAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.ContinuesToBeTrue ?? throw new AutomationBuilderException("when clause must be defined"),
            info.For ?? TimeSpan.Zero,
            info.Execution ?? throw new AutomationBuilderException("execution must be defined"))
            .WithMeta(GetMeta(info));
    }
    
    public static IConditionalAutomation Build(this ConditionalAutomationWithServicesBuildingInfo info)
    {
        return new ConditionalAutomationWithServices(
            info._services,
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.ContinuesToBeTrueWithServices ?? throw new AutomationBuilderException("when clause must be defined"),
            info.For ?? TimeSpan.Zero,
            info.ExecutionWithServices ?? throw new AutomationBuilderException("execution must be defined"),
            GetMeta(info)
        );
    }

    private static AutomationMetaData GetMeta(AutomationBuildingInfo info)
    {
        return new AutomationMetaData()
        {
            Name = info.Name ?? nameof(SimpleAutomationWithServices),
            Description = info.Description,
            Enabled = info.EnabledAtStartup,
            Id = Guid.NewGuid(),
        };
    }
}
