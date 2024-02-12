namespace HaKafkaNet;

public interface IAutomationFactory
{
    IHaServices Services { get; }

    SimpleAutomation SimpleAutomation(
        IEnumerable<string> triggerEntities,
        Func<HaEntityStateChange, CancellationToken, Task> execute,
        EventTiming eventTimings = EventTiming.PostStartup);
    ConditionalAutomation ConditionalAutomation(
        IEnumerable<string> triggerEntities,
        Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue,
        TimeSpan @for,
        Func<CancellationToken, Task> execute);

    SchedulableAutomation CreateScheduled(
        IEnumerable<string> triggerIds, 
        GetNextEventFromEntityState getNextEvent,
        Func<CancellationToken, Task> execution,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false,
        EventTiming timngs = EventTiming.PostStartup
    );

    ConditionalAutomation EntityAutoOff(string entity_id, TimeSpan timeToLeaveOn);
    ConditionalAutomation EntityAutoOff(string entity_id, int minutes);
    LightOnMotionAutomation LightOnMotion(string motionId, string lightId);
    LightOnMotionAutomation LightOnMotion(IEnumerable<string> motionId, IEnumerable<string> lightId);
    LightOffOnNoMotion LightOffOnNoMotion(string motionId, string lightId, TimeSpan duration);
    LightOffOnNoMotion LightOffOnNoMotion(IEnumerable<string> motionIds, IEnumerable<string> lightIds, TimeSpan duration);
    SunDawnAutomation SunDawnAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true);
    SunRiseAutomation SunRiseAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true);
    SunNoonAutomation SunSNoonAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true);
    SunDuskAutomation SunDuskAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true);
    SunSetAutomation SunSetAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true);
    SunMidnightAutomation SunMidnightAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null, EventTiming timings = SunAutomation.DEFAULT_SUN_EVENT_TIMINGS, bool executePast = true);
}
