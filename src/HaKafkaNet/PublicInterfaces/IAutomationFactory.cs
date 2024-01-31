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

    LightOnMotionAutomation LightOnMotion(string motionId, string lightId);
    LightOnMotionAutomation LightOnMotion(IEnumerable<string> motionId, IEnumerable<string> lightId);
    LightOffOnNoMotion LightOffOnNoMotion(string motionId, string lightId, TimeSpan duration);
    LightOffOnNoMotion LightOffOnNoMotion(IEnumerable<string> motionIds, IEnumerable<string> lightIds, TimeSpan duration);

}
