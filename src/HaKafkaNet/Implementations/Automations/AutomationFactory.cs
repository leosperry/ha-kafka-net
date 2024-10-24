
namespace HaKafkaNet;

internal class AutomationFactory : IAutomationFactory
{
    readonly IHaServices _services;   

    public AutomationFactory(IHaServices services)
    {
        _services = services;
    }

    public IHaServices Services
    {
        get => _services;
    }

    public ConditionalAutomation ConditionalAutomation(
        IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue, 
        TimeSpan @for, Func<CancellationToken, Task> execute)
    {
        return new ConditionalAutomation(triggerEntities, continuesToBeTrue, @for, execute);
    }

    public SchedulableAutomation CreateScheduled(
        IEnumerable<string> triggerIds, 
        GetNextEventFromEntityState getNextEvent,
        Func<CancellationToken, Task> execution,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false,
        EventTiming timngs = EventTiming.PostStartup,
        bool reschedudulable = false
    )
    {
        var scheduled = new SchedulableAutomation(triggerIds, getNextEvent, execution, shouldExecutePastEvents, shouldExecuteOnError, reschedudulable)
        {
            EventTimings = timngs,
        };
        return scheduled;
    }

    public SchedulableAutomation CreateDurable(
        IEnumerable<string> triggerIds, 
        GetNextEventFromEntityState getNextEvent,
        Func<CancellationToken, Task> execution,
        bool shouldExecutePastEvents = true,
        bool shouldExecuteOnError = false,
        bool reschedudulable = false
    )
    {
        var scheduled = new SchedulableAutomation(triggerIds, getNextEvent, execution, shouldExecutePastEvents, shouldExecuteOnError)
        {
            IsReschedulable = reschedudulable,
            EventTimings = EventTiming.Durable
        };
        return scheduled;    
    }

    public SimpleAutomation SimpleAutomation(IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task> execute, 
        EventTiming eventTimings = EventTiming.PostStartup)
    {
        return new SimpleAutomation(triggerEntities, execute, eventTimings);
    }

    public SimpleAutomation<Tstate, Tatt> CreateSimple<Tstate, Tatt>(
        IEnumerable<string> triggerIds, 
        Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task> execute, 
        EventTiming eventTiming = EventTiming.PostStartup)
    {
        return new SimpleAutomation<Tstate, Tatt>(triggerIds, execute, eventTiming);
    }

    public ConditionalAutomation EntityAutoOff(string entity_id, TimeSpan timeToLeaveOn)
    {
        if (timeToLeaveOn < TimeSpan.Zero)
        {
            throw new HaKafkaNetException("LightAutoOff: timeToLeaveOn cannot be negative");
        }
        return new ConditionalAutomation(
            [entity_id],
            (sc,ct)=> Task.FromResult(sc.New.GetStateEnum<OnOff>() == OnOff.On),
            timeToLeaveOn, 
            ct => _services.Api.TurnOff(entity_id, ct));
    }

    public ConditionalAutomation EntityAutoOff(string lightId, int minutes)
        => EntityAutoOff(lightId, TimeSpan.FromMinutes(minutes));

    public SchedulableAutomation DurableAutoOn(string entityId, TimeSpan timeToLeaveOff)
    {
        if (timeToLeaveOff < TimeSpan.Zero)
        {
            throw new ArgumentException($"{nameof(timeToLeaveOff)} cannot be negative", nameof(timeToLeaveOff));
        }
        return new SchedulableAutomation([entityId],
             (sc, ct) =>{
                if (sc.ToOnOff().IsOff())
                {
                    return Task.FromResult<DateTime?>(sc.New.LastUpdated + timeToLeaveOff);
                }
                return Task.FromResult(default(DateTime?));
            },
            ct => _services.Api.TurnOn(entityId), true,false,false)
            {
                EventTimings = EventTiming.Durable
            };
    }

    public SchedulableAutomation DurableAutoOff(string entityId, TimeSpan timeToLeaveOn)
    {
        if (timeToLeaveOn < TimeSpan.Zero)
        {
            throw new ArgumentException($"{nameof(timeToLeaveOn)} cannot be negative", nameof(timeToLeaveOn));
        }
        return new SchedulableAutomation([entityId],
             (sc, ct) =>{
                if (sc.ToOnOff().IsOn())
                {
                    return Task.FromResult<DateTime?>(sc.New.LastUpdated + timeToLeaveOn);
                }
                return Task.FromResult(default(DateTime?));
            },ct => _services.Api.TurnOff(entityId), true)
            {
                EventTimings = EventTiming.Durable
            };
    }

    public SchedulableAutomation DurableAutoOffOnEntityOff(string entityToTurnOff, string triggerEntity, TimeSpan timeToLeaveOn)
    {
        return DurableAutoOffOnEntityOff([entityToTurnOff], triggerEntity, timeToLeaveOn);
    }

    public SchedulableAutomation DurableAutoOffOnEntityOff(IEnumerable<string> entitiesToTurnOff, string triggerEntity, TimeSpan timeToLeaveOn)
    {
        if (timeToLeaveOn < TimeSpan.Zero)
        {
            throw new ArgumentException($"{nameof(timeToLeaveOn)} cannot be negative", nameof(timeToLeaveOn));
        }
        return new SchedulableAutomation([triggerEntity],
             (sc, ct) =>{
                if (sc.ToOnOff().IsOff())
                {
                    return Task.FromResult<DateTime?>(sc.New.LastUpdated + timeToLeaveOn);
                }
                return Task.FromResult<DateTime?>(null);
            },ct => _services.Api.TurnOff(entitiesToTurnOff), true)
            {
                EventTimings = EventTiming.Durable,
            };    
    }

    public LightOnMotionAutomation LightOnMotion(string motionId, string lightId)
    {
        return new LightOnMotionAutomation([motionId], [lightId], _services);
    }

    public LightOnMotionAutomation LightOnMotion(IEnumerable<string> motionId, IEnumerable<string> lightId)
    {
        return new LightOnMotionAutomation(motionId, lightId, _services);
    }

    public LightOffOnNoMotion LightOffOnNoMotion(string motionId, string lightId, TimeSpan duration)
    {
        return new LightOffOnNoMotion([motionId],[lightId], duration, _services);
    }

    public LightOffOnNoMotion LightOffOnNoMotion(IEnumerable<string> motionIds, IEnumerable<string> lightIds, TimeSpan duration)
    {
        return new LightOffOnNoMotion(motionIds, lightIds, duration, _services);
    }

    /// <summary>
    /// Requires Home Assistant to have sun.sun configured in Kafka Integration
    /// May not work in arctic circle
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="offset">Positive or negative offset from Sunrise</param>
    /// <returns></returns>
    public SunDawnAutomation SunDawnAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
    {
        return new SunDawnAutomation(execution, offset, timings, executePast);
    }

    /// <summary>
    /// Requires Home Assistant to have sun.sun configured in Kafka Integration
    /// May not work in arctic circle
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="offset">Positive or negative offset from Sunrise</param>
    /// <returns></returns>
    public SunRiseAutomation SunRiseAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
    {
        return new SunRiseAutomation(execution, offset, timings, executePast);
    }

    /// <summary>
    /// Requires Home Assistant to have sun.sun configured in Kafka Integration
    /// May not work in arctic circle
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="offset">Positive or negative offset from Sunset</param>
    /// <returns></returns>
    public SunNoonAutomation SunNoonAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
    {
        return new SunNoonAutomation(execution, offset, timings, executePast);
    }

    /// <summary>
    /// Requires Home Assistant to have sun.sun configured in Kafka Integration
    /// May not work in arctic circle
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="offset">Positive or negative offset from Sunset</param>
    /// <returns></returns>
    public SunSetAutomation SunSetAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
    {
        return new SunSetAutomation(execution, offset, timings, executePast);
    }

    /// <summary>
    /// Requires Home Assistant to have sun.sun configured in Kafka Integration
    /// May not work in arctic circle
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="offset">Positive or negative offset from Sunset</param>
    /// <returns></returns>
    public SunDuskAutomation SunDuskAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
    {
        return new SunDuskAutomation(execution, offset, timings, executePast);
    }

    /// <summary>
    /// Requires Home Assistant to have sun.sun configured in Kafka Integration
    /// May not work in arctic circle
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="offset">Positive or negative offset from Sunset</param>
    /// <returns></returns>
    public SunMidnightAutomation SunMidnightAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = EventTiming.Durable, bool executePast = true)
    {
        return new SunMidnightAutomation(execution, offset, timings, executePast);
    }

    public SimpleAutomation EntityOnOffWithAnother(string primaryEntityId, params string[] secondaries)
    {
        return new SimpleAutomation([primaryEntityId],
            (sc, ct) =>{
                var onOffState = sc.ToOnOff();
                return onOffState.New.State switch
                {
                    OnOff.On => _services.Api.TurnOn(secondaries),
                    OnOff.Off => _services.Api.TurnOff(secondaries),
                    _ => Task.CompletedTask
                };

            }, EventTiming.PostStartup);
    }

    public SimpleAutomation EntityOnOffOppositeAnother(string primaryEntityId, params string[] secondaries)
    {
        return new SimpleAutomation([primaryEntityId],
            (sc, ct) =>{
                var onOffState = sc.ToOnOff();
                return onOffState.New.State switch
                {
                    OnOff.On => _services.Api.TurnOff(secondaries),
                    OnOff.Off => _services.Api.TurnOn(secondaries),
                    _ => Task.CompletedTask
                };

            }, EventTiming.PostStartup);
    }
}
