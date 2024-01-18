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

    //TODO:
    // IAutomation AfterNoMotionForTime_TurnOffLights(IEnumerable<string> motionEntities, IEnumerable<string> lightEntities);
    // IAutomation OnMotion_TurnOnLight(IEnumerable<string> motionEntities, IEnumerable<string> lightEntities);
}
