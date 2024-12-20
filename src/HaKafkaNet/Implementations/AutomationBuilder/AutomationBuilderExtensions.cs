﻿namespace HaKafkaNet;
public static partial class AutomationBuilderExtensions
{
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

    /// <summary>
    /// Sets TriggerOnBadState to true (defaults to false)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <returns></returns>
    public static T TriggerOnBadState<T>(this T info) where T : AutomationBuildingInfo
    {
        info.TriggerOnBadState = true;
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

    public static T WithMode<T>(this T info, AutomationMode mode) where T : AutomationBuildingInfo
    {
        info.Mode = mode;
        return info;
    }


    public static SimpleAutomationBuildingInfo WithExecution(this SimpleAutomationBuildingInfo info, Func<HaEntityStateChange, CancellationToken, Task> execution)
    {
        info.Execution = execution;
        return info;
    }

    public static TypedAutomationBuildingInfo<Tstate, Tatt> WithExecution<Tstate, Tatt>(this TypedAutomationBuildingInfo<Tstate, Tatt> info,
        Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>> , CancellationToken, Task> execution)
    {
        info.Execution = execution;
        return info;
    }

    public static IAutomation Build(this SimpleAutomationBuildingInfo info)
    {
        return new SimpleAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.EventTimings ?? EventTiming.PostStartup).WithMeta(GetMeta(info));
    }

    public static IAutomation<Tstate, Tatt> Build<Tstate, Tatt>(this TypedAutomationBuildingInfo<Tstate, Tatt> info)
    {
        return new SimpleAutomation<Tstate, Tatt>(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.EventTimings ?? EventTiming.PostStartup).WithMeta(GetMeta(info));
    }

    public static IConditionalAutomation Build(this ConditionalAutomationBuildingInfo info)
    {
        var conditional = new ConditionalAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.ContinuesToBeTrue ?? throw new AutomationBuilderException("when clause must be specified"),
            info.For ?? TimeSpan.Zero,
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"))
            .WithMeta(GetMeta(info));

        conditional.ShouldExecuteOnContinueError = info.ShouldExecuteOnContinueError;
        conditional.ShouldExecutePastEvents = info.ShouldExecutePastEvents;
        conditional.EventTimings = info.EventTimings ?? EventTiming.PostStartup;

        return conditional;
    }

    public static IConditionalAutomation<Tstate, Tatt> Build<Tstate, Tatt>(this TypedConditionalBuildingInfo<Tstate, Tatt> info)
    {
        var conditional = new ConditionalAutomation<Tstate,Tatt>(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(), 
            info.For ?? throw new AutomationBuilderException("must define for"), 
            info.ContinuesToBeTrue ?? throw new AutomationBuilderException("must define ContinuesToBeTrue"), 
            info.Execution ?? throw new AutomationBuilderException("must define execution"));
        
        conditional.ShouldExecuteOnContinueError = info.ShouldExecuteOnContinueError;
        conditional.ShouldExecutePastEvents = info.ShouldExecutePastEvents;
        conditional.EventTimings = info.EventTimings ?? EventTiming.PostStartup;

        conditional.SetMeta(GetMeta(info));

        return conditional;
    }

    public static ISchedulableAutomation Build(this SchedulableAutomationBuildingInfo info)
    {
        var auto = new SchedulableAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            Get_GetNextScheduled(info),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.ShouldExecutePastEvents,
            info.ShouldExecuteOnContinueError)
            .WithMeta(GetMeta(info));
        auto.EventTimings = info.EventTimings ?? EventTiming.PostStartup;
        auto.IsReschedulable = info.IsReschedulable;
                
        return auto;
    }

    public static ISchedulableAutomation<Tstate, Tatt> Build<Tstate, Tatt>(this TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> info)
    {
        var auto = new SchedulableAutomation<Tstate, Tatt>(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            Get_GetNextScheduled(info),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.ShouldExecutePastEvents,
            info.ShouldExecuteOnContinueError
        );
        auto.EventTimings = info.EventTimings ?? EventTiming.PostStartup;
        auto.IsReschedulable = info.IsReschedulable;

        auto.SetMeta(GetMeta(info));

        return auto;
    }

    private static GetNextEventFromEntityState Get_GetNextScheduled(SchedulableAutomationBuildingInfo info)
    {
        if (info.GetNextScheduled is not null && (info.WhileCondition is not null || info.For is not null))
        {
            throw new AutomationBuilderException("cannot specify both GetNextScheduled callback and (WhileCondition or ForTime)");
        }
        if (info.GetNextScheduled is not null)
        {
            return info.GetNextScheduled;
        }

        if (info.WhileCondition is null || info.For is null)
        {
            throw new AutomationBuilderException("ForCondition specified, but ForTime is not");
        }

        return new GetNextEventFromEntityState((sc, ct) => {
            if (info.WhileCondition(sc))
            {
                return Task.FromResult<DateTime?>(DateTime.Now.Add(info.For.Value));
            }
            return Task.FromResult<DateTime?>(null);
        });
    }

    private static GetNextEventFromEntityState<Tstate, Tatt> Get_GetNextScheduled<Tstate, Tatt>(TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> info)
    {
        if (info.GetNextScheduled is not null && (info.WhileCondition is not null || info.For is not null))
        {
            throw new AutomationBuilderException("cannot specify both GetNextScheduled callback and (WhileCondition or ForTime)");
        }
        if (info.GetNextScheduled is not null)
        {
            return info.GetNextScheduled;
        }

        if (info.WhileCondition is null || info.For is null)
        {
            throw new AutomationBuilderException("ForCondition specified, but ForTime is not");
        }

        return new GetNextEventFromEntityState<Tstate, Tatt>((sc, ct) => {
            if (info.WhileCondition(sc))
            {
                return Task.FromResult<DateTime?>(DateTime.Now.Add(info.For.Value));
            }
            return Task.FromResult<DateTime?>(null);
        });
    }

    public static SunAutomation Build(this SunAutomationBuildingInfo info)
    {
        SunAutomation auto = info.SunEvent switch
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
        auto.WithMeta(GetMeta(info));
        return auto;
    }

    public static SunAutomationBuildingInfo WithOffset(this SunAutomationBuildingInfo info, TimeSpan offset)
    {
        info.Offset = offset;
        return info;
    }

    public static SunAutomationBuildingInfo ExecutePastEvents(this SunAutomationBuildingInfo info, bool executePast)
    {
        info.ExecutePast = executePast;
        return info;
    }

    public static SunAutomationBuildingInfo WithExecution(this SunAutomationBuildingInfo info, Func<CancellationToken, Task> execution)
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
            AdditionalEntitiesToTrack = info is MostAutomationsBuildingInfo most ? most.AdditionalEntitiesToTrack : null,
            TriggerOnBadState = info.TriggerOnBadState,
            Mode = info.Mode
        };
    }

    public static TypedAutomationBuildingInfo<DateTime?, SceneControllerEvent> CreateSceneController(this IAutomationBuilder bldr, bool enabledAtStartup = true)
    {
        return bldr.CreateSimple<DateTime?, SceneControllerEvent>(enabledAtStartup);
    }
}
