namespace HaKafkaNet;

public interface IAutomationFactory
{
    IAutomation SimpleAutomation(
        IEnumerable<string> triggerEntities,
        Func<HaEntityStateChange, CancellationToken, Task> execute,
        EventTiming eventTimings = EventTiming.PostStartup);
    IAutomation ConditionalAutomation(
        IEnumerable<string> triggerEntities,
        Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue,
        TimeSpan @for,
        Func<CancellationToken, Task> execute);

    IAutomation LightOnMotion(string motionId, string lightId);
    IAutomation LightOnMotion(IEnumerable<string> motionId, IEnumerable<string> lightId);
    IConditionalAutomation LightOffOnNoMotion(string motionId, string lightId, TimeSpan duration);
    IConditionalAutomation LightOffOnNoMotion(IEnumerable<string> motionIds, IEnumerable<string> lightIds, TimeSpan duration);

}
