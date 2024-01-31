
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class AutomationFactory : IAutomationFactory
{
    readonly IHaServices _services;
    readonly ILogger<ConditionalAutomationWrapper> _logger;

    public AutomationFactory(IHaServices services ,ILogger<ConditionalAutomationWrapper> logger)
    {
        _services = services;
        _logger = logger;
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

    public SimpleAutomation SimpleAutomation(IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task> execute, 
        EventTiming eventTimings = EventTiming.PostStartup)
    {
        return new SimpleAutomation(triggerEntities, execute, eventTimings);
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
}
