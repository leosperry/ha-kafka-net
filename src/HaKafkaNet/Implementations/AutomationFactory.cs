
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

    public IAutomation ConditionalAutomation(IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue, TimeSpan @for, Func<CancellationToken, Task> execute)
    {
        var ca =  new ConditionalAutomation(triggerEntities, continuesToBeTrue, @for, execute);
        return new ConditionalAutomationWrapper(ca, _logger);
    }

    public IAutomation SimpleAutomation(IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task> execute, EventTiming eventTimings = EventTiming.PostStartup)
    {
        return new SimpleAutomation(triggerEntities, execute, eventTimings);
    }

    public IAutomation LightOnMotion(string motionId, string lightId)
    {
        return new LightOnMotionAutomation([motionId], [lightId], _services);
    }

    public IAutomation LightOnMotion(IEnumerable<string> motionId, IEnumerable<string> lightId)
    {
        return new LightOnMotionAutomation(motionId, lightId, _services);
    }

    public IConditionalAutomation LightOffOnNoMotion(string motionId, string lightId, TimeSpan duration)
    {
        return new LightOffOnNoMotion([motionId],[lightId], duration, _services);
    }

    public IConditionalAutomation LightOffOnNoMotion(IEnumerable<string> motionIds, IEnumerable<string> lightIds, TimeSpan duration)
    {
        return new LightOffOnNoMotion(motionIds, lightIds, duration, _services);
    }
}
