namespace HaKafkaNet;

/// <summary>
/// Methods for adding functionality to automations
/// </summary>
public static partial class AutomationBuilderExtensions
{
    /// <summary>
    /// Sets the name that will appear on the dashboard
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T WithName<T>(this T info, string name) where T : AutomationBuildingInfo
    {
        info.Name = name;
        return info;
    }

    /// <summary>
    /// Sets the Description that will appear on the dashboard
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static T WithDescription<T>(this T info, string description) where T : AutomationBuildingInfo
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

    /// <summary>
    /// Sets the triggering entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="triggerEntityIds"></param>
    /// <returns></returns>
    public static T WithTriggers<T>(this T info, params string[] triggerEntityIds) where T : MostAutomationsBuildingInfo
    {
        info.TriggerEntityIds = triggerEntityIds;
        return info;
    }

    /// <summary>
    /// Tells the system monitor that additional entities are important
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="entityIds"></param>
    /// <returns></returns>
    public static T WithAdditionalEntitiesToTrack<T>(this T info, params string[] entityIds) where T : MostAutomationsBuildingInfo
    {
        info.AdditionalEntitiesToTrack = entityIds;
        return info;
    }

    /// <summary>
    /// Sets the timings of the automation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="timings"></param>
    /// <returns></returns>
    public static T WithTimings<T>(this T info, EventTiming timings) where T : AutomationBuildingInfo
    {
        info.EventTimings = timings;
        return info;
    }

    /// <summary>
    /// Sets the automation mode
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static T WithMode<T>(this T info, AutomationMode mode) where T : AutomationBuildingInfo
    {
        info.Mode = mode;
        return info;
    }

    /// <summary>
    /// Sets IsActive to true and EventTimings to Durable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public static T MakeActive<T>(this T info, bool isActive = true) where T : AutomationBuildingInfo
    {
        info.IsActive = isActive;
        info.EventTimings = EventTiming.Durable;
        return info;
    }

    /// <summary>
    /// Sets the Execute method of the automation
    /// </summary>
    /// <param name="info"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public static SimpleAutomationBuildingInfo WithExecution(this SimpleAutomationBuildingInfo info, Func<HaEntityStateChange, CancellationToken, Task> execution)
    {
        info.Execution = execution;
        return info;
    }

    /// <summary>
    /// Sets the Execute method of the automation
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="info"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public static TypedAutomationBuildingInfo<Tstate, Tatt> WithExecution<Tstate, Tatt>(this TypedAutomationBuildingInfo<Tstate, Tatt> info,
        Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task> execution)
    {
        info.Execution = execution;
        return info;
    }

    /// <summary>
    /// Builds the automation
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    /// <exception cref="AutomationBuilderException"></exception>
    public static IAutomation Build(this SimpleAutomationBuildingInfo info)
    {
        var auto = new SimpleAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.EventTimings ?? EventTiming.PostStartup).WithMeta(GetMeta(info));
        auto.IsActive = info.IsActive;

        return auto;
    }

    /// <summary>
    /// Builds the automation
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="info"></param>
    /// <returns></returns>
    /// <exception cref="AutomationBuilderException"></exception>
    public static IAutomation<Tstate, Tatt> Build<Tstate, Tatt>(this TypedAutomationBuildingInfo<Tstate, Tatt> info)
    {
        var auto = new SimpleAutomation<Tstate, Tatt>(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.EventTimings ?? EventTiming.PostStartup).WithMeta(GetMeta(info));

        auto.IsActive = info.IsActive;

        return auto;
    }

    /// <summary>
    /// Builds the automation
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    /// <exception cref="AutomationBuilderException"></exception>
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

        conditional.IsActive = info.IsActive;

        return conditional;
    }


    /// <summary>
    /// Builds the automation
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="info"></param>
    /// <returns></returns>
    /// <exception cref="AutomationBuilderException"></exception>
    public static IConditionalAutomation<Tstate, Tatt> Build<Tstate, Tatt>(this TypedConditionalBuildingInfo<Tstate, Tatt> info)
    {
        var conditional = new ConditionalAutomation<Tstate, Tatt>(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            info.For ?? throw new AutomationBuilderException("must define for"),
            info.ContinuesToBeTrue ?? throw new AutomationBuilderException("must define ContinuesToBeTrue"),
            info.Execution ?? throw new AutomationBuilderException("must define execution"));

        conditional.ShouldExecuteOnContinueError = info.ShouldExecuteOnContinueError;
        conditional.ShouldExecutePastEvents = info.ShouldExecutePastEvents;
        conditional.EventTimings = info.EventTimings ?? EventTiming.PostStartup;

        conditional.SetMeta(GetMeta(info));

        conditional.IsActive = info.IsActive;

        return conditional;
    }


    /// <summary>
    /// Builds the automation
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    /// <exception cref="AutomationBuilderException"></exception>
    public static ISchedulableAutomation Build(this SchedulableAutomationBuildingInfo info)
    {
        var auto = new SchedulableAutomation(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            Get_GetNextScheduled(info),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.ShouldExecutePastEvents,
            info.ShouldExecuteOnContinueError, info.IsReschedulable)
            .WithMeta(GetMeta(info));
        auto.EventTimings = info.EventTimings ?? EventTiming.PostStartup;
        auto.IsReschedulable = info.IsReschedulable;

        auto.IsActive = info.IsActive;

        return auto;
    }

    /// <summary>
    /// Builds the automation
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="info"></param>
    /// <returns></returns>
    /// <exception cref="AutomationBuilderException"></exception>
    public static ISchedulableAutomation<Tstate, Tatt> Build<Tstate, Tatt>(this TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> info)
    {
        var auto = new SchedulableAutomation<Tstate, Tatt>(
            info.TriggerEntityIds ?? Enumerable.Empty<string>(),
            Get_GetNextScheduled(info),
            info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
            info.IsReschedulable,
            info.ShouldExecutePastEvents,
            info.ShouldExecuteOnContinueError
        );
        auto.EventTimings = info.EventTimings ?? EventTiming.PostStartup;
        auto.IsReschedulable = info.IsReschedulable;

        auto.IsActive = info.IsActive;

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

        return new GetNextEventFromEntityState((sc, ct) =>
        {
            if (info.WhileCondition(sc))
            {
                return Task.FromResult<DateTimeOffset?>(info.TimeProvider.GetLocalNow().LocalDateTime.Add(info.For.Value));
            }
            return Task.FromResult<DateTimeOffset?>(null);
        });
    }

    private static GetNextEventFromEntityState<Tstate, Tatt> Get_GetNextScheduled<Tstate, Tatt>(TypedSchedulableAutomationBuildingInfo<Tstate, Tatt> info)
    {
        if (info.GetNextScheduledInternal is not null && (info.WhileCondition is not null || info.For is not null))
        {
            throw new AutomationBuilderException("cannot specify both GetNextScheduled callback and (WhileCondition or ForTime)");
        }
        if (info.GetNextScheduledInternal is not null)
        {
            return info.GetNextScheduledInternal;
        }

        if (info.WhileCondition is null || info.For is null)
        {
            throw new AutomationBuilderException("ForCondition specified, but ForTime is not");
        }

        return new GetNextEventFromEntityState<Tstate, Tatt>((sc, ct) =>
        {
            if (info.WhileCondition(sc))
            {
                return Task.FromResult<DateTimeOffset?>(info.TimeProvider.GetLocalNow().LocalDateTime.Add(info.For.Value));
            }
            return Task.FromResult<DateTimeOffset?>(null);
        });
    }

    /// <summary>
    /// Builds the automation
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    /// <exception cref="AutomationBuilderException"></exception>
    public static SunAutomation Build(this SunAutomationBuildingInfo info)
    {
        SunAutomation auto = info.SunEvent switch
        {
            SunEventType.Dawn => new SunDawnAutomation(info.TimeProvider,
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Rise => new SunRiseAutomation(info.TimeProvider,
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Noon => new SunNoonAutomation(info.TimeProvider,
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Set => new SunSetAutomation(info.TimeProvider,
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Dusk => new SunDuskAutomation(info.TimeProvider,
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            SunEventType.Midnight => new SunMidnightAutomation(info.TimeProvider,
                info.Execution ?? throw new AutomationBuilderException("execution must be specified"),
                info.Offset, info.EventTimings ?? EventTiming.Durable, info.ExecutePast),
            _ => throw new AutomationBuilderException("Unknown sun type. This should not be possible")
        };
        auto.WithMeta(GetMeta(info));

        auto.IsActive = info.IsActive;

        return auto;
    }

    /// <summary>
    /// sets the time relative to the sun event when the automation should execute
    /// </summary>
    /// <param name="info"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static SunAutomationBuildingInfo WithOffset(this SunAutomationBuildingInfo info, TimeSpan offset)
    {
        info.Offset = offset;
        return info;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="executePast"></param>
    /// <returns></returns>
    public static SunAutomationBuildingInfo ExecutePastEvents(this SunAutomationBuildingInfo info, bool executePast)
    {
        info.ExecutePast = executePast;
        return info;
    }

    /// <summary>
    /// Sets the Execute method
    /// </summary>
    /// <param name="info"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Very handy for multi-button scene controllers or smart switches with scene controller functionality
    /// See:
    /// https://github.com/leosperry/ha-kafka-net/wiki/Scene-Controllers
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="enabledAtStartup"></param>
    /// <returns></returns>
    public static TypedAutomationBuildingInfo<DateTime?, SceneControllerEvent> CreateSceneController(this IAutomationBuilder builder, bool enabledAtStartup = true)
    {
        return builder.CreateSimple<DateTime?, SceneControllerEvent>(enabledAtStartup);
    }
    
    /// <summary>
    /// Stongly type for use with NFC tags
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="enabledAtStartup"></param>
    /// <returns></returns>
    public static TypedAutomationBuildingInfo<DateTime?, TagAttributes> CreateNfcTagAutomation(this IAutomationBuilder builder, bool enabledAtStartup = true)
    {
        return builder.CreateSimple<DateTime?, TagAttributes>(enabledAtStartup);
    }
    
    
}
