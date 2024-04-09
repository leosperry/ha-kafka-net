namespace HaKafkaNet;
public static partial class AutomationBuilderExtensions
{
    [Obsolete("Id has been replaced with key. This method does nothing", false)]
    public static T WithId<T>(this T info, Guid id) where T: AutomationBuildingInfo
    {
        //info.Id = id;
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

    public static T WithTriggers<T>(this T info, params string[] triggerEntityIds) where T: MostAutomationsBuildingInfo
    {
        info.TriggerEntityIds = triggerEntityIds;
        return info;
    }

    public static T WithAdditionalEntitiesToTrack<T>(this T info, params string[] entityIds) where T : MostAutomationsBuildingInfo
    {
        info.AdditionalEntitiesToTrack = entityIds;
        return info;
    }

    public static T WithTimings<T>(this T info, EventTiming timings) where T : AutomationBuildingInfo
    {
        info.EventTimings = timings;
        return info;
    }


    public static SimpleAutomationBuildingInfo WithExecution(this SimpleAutomationBuildingInfo info, Func<HaEntityStateChange, CancellationToken, Task> execution)
    {
        info.Execution = execution;
        return info;
    } 

    [Obsolete("", false)]
    public static SimpleAutomationWithServicesBuildingInfo WithExecution(this SimpleAutomationWithServicesBuildingInfo info, Func<IHaServices, HaEntityStateChange, CancellationToken, Task> execution)
    {
        info.ExecutionWithServcies = execution;
        return info;
    }    


    public static IAutomation Build(this SimpleAutomationBuildingInfo info)
    {
        return new SimpleAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.EventTimings ?? EventTiming.PostStartup).WithMeta(GetMeta(info));

    }
    
    [Obsolete("", false)]
    public static IAutomation Build(this SimpleAutomationWithServicesBuildingInfo info)
    {
        return new SimpleAutomationWithServices(
            info._services,
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.ExecutionWithServcies ?? throw new AutomationBuilderException("execution must be specified"),
            info.EventTimings ?? EventTiming.PostStartup).WithMeta(GetMeta(info));
    }

    public static IConditionalAutomation Build(this ConditionalAutomationBuildingInfo info)
    {
        return new ConditionalAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.ContinuesToBeTrue ?? throw new AutomationBuilderException("when clause must be specified"),
            info.For ?? TimeSpan.Zero,
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"))
            .WithMeta(GetMeta(info));
    }
    
    [Obsolete("", false)]
    public static IConditionalAutomation Build(this ConditionalAutomationWithServicesBuildingInfo info)
    {
        return new ConditionalAutomationWithServices(
            info._services,
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.ContinuesToBeTrueWithServices ?? throw new AutomationBuilderException("when clause must be specified"),
            info.For ?? TimeSpan.Zero,
            info.ExecutionWithServices ?? throw new AutomationBuilderException("execution must be specified"))
            .WithMeta(GetMeta(info));
    }

    public static ISchedulableAutomation Build(this SchedulableAutomationBuildingInfo info)
    {
        var auto = new SchedulableAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.GetNextScheduled ?? throw new AutomationBuilderException("GetNextScheduled must be specified"),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.ShouldExecutePastEvents,
            info.ShouldExecuteOnContinueError)
            .WithMeta(GetMeta(info));
        auto.EventTimings = info.EventTimings ?? EventTiming.PostStartup;
        auto.IsReschedulable = info.IsReschedulable;
        return auto;
    }

    public static SunAutomation Build(this SunAutommationBuildingInfo info)
    {
        return info.SunEvent switch
        {
            SunEventType.Dawn => new SunDawnAutomation(
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"), 
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Rise => new SunRiseAutomation(
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"), 
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Noon => new SunNoonAutomation(
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"), 
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Set => new SunSetAutomation(
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"), 
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Dusk => new SunDuskAutomation(
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"), 
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Midnight => new SunMidnightAutomation(
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"), 
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            _ => throw new AutomationBuilderException("Unknown sun type. This should not be possible")
        };
    }

    public static SunAutommationBuildingInfo WithOffset(this SunAutommationBuildingInfo info, TimeSpan offset)
    {
        info.Offset = offset;
        return info;
    }

    public static SunAutommationBuildingInfo ExecutePastEvents(this SunAutommationBuildingInfo info, bool executePast)
    {
        info.ExecutePast = executePast;
        return info;
    }

    public static SunAutommationBuildingInfo WithExecution(this SunAutommationBuildingInfo info, Func<CancellationToken, Task> execution)
    {
        info.Execution = execution;
        return info;
    }

    private static AutomationMetaData GetMeta(AutomationBuildingInfo info)
    {
        return new AutomationMetaData()
        {
            Name = info.Name ?? $"from builder {info.GetType().Name}",
            Description = info.Description,
            Enabled = info.EnabledAtStartup,
            AdditionalEntitiesToTrack = info is MostAutomationsBuildingInfo most ? most.AdditionalEntitiesToTrack : null
        };
    }
}
