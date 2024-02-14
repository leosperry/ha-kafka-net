﻿
using Microsoft.Extensions.Logging;

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
        EventTiming timngs = EventTiming.PostStartup
    )
    {
        var scheduled = new SchedulableAutomation(triggerIds, getNextEvent, execution, shouldExecutePastEvents, shouldExecuteOnError);
        scheduled.EventTimings = timngs;
        return scheduled;
    }

    public SimpleAutomation SimpleAutomation(IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task> execute, 
        EventTiming eventTimings = EventTiming.PostStartup)
    {
        return new SimpleAutomation(triggerEntities, execute, eventTimings);
    }

    public ConditionalAutomation EntityAutoOff(string entity_id, TimeSpan timeToLeaveOn)
    {
        if (timeToLeaveOn < TimeSpan.Zero)
        {
            throw new HaKafkaNetException("LightAutoOff: timeToLeaveOn cannot be negative");
        }
        return new ConditionalAutomation(
            [entity_id],
            (sc,ct)=> Task.FromResult(sc.New.State == "on"), 
            timeToLeaveOn, 
            ct => _services.Api.TurnOff(entity_id));
    }

    public ConditionalAutomation EntityAutoOff(string lightId, int minutes)
        => EntityAutoOff(lightId, TimeSpan.FromMinutes(minutes));
    

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
    public SunDawnAutomation SunDawnAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true)
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
    public SunRiseAutomation SunRiseAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true)
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
    public SunNoonAutomation SunSNoonAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true)
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
    public SunSetAutomation SunSetAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true)
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
    public SunDuskAutomation SunDuskAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true)
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
    public SunMidnightAutomation SunMidnightAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true)
    {
        return new SunMidnightAutomation(execution, offset, timings, executePast);
    }

    
}