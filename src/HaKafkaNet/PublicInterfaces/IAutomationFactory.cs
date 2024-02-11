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

    LightOnMotionAutomation LightOnMotion(string motionId, string lightId);
    LightOnMotionAutomation LightOnMotion(IEnumerable<string> motionId, IEnumerable<string> lightId);
    LightOffOnNoMotion LightOffOnNoMotion(string motionId, string lightId, TimeSpan duration);
    LightOffOnNoMotion LightOffOnNoMotion(IEnumerable<string> motionIds, IEnumerable<string> lightIds, TimeSpan duration);
    SunDawnAutomation SunDawnAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null);
    SunRiseAutomation SunRiseAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null);
    SunNoonAutomation SunSNoonAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null);
    SunDuskAutomation SunDuskAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null);
    SunSetAutomation SunSetAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null);
    SunMidnightAutomation SunMidnightAutomation(Func<CancellationToken, Task> execution, TimeSpan? offset = null);




}
