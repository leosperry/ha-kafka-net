
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class AutomationFactory : IAutomationFactory
{
    ILogger<ConditionalAutomationWrapper> _logger;

    public AutomationFactory(ILogger<ConditionalAutomationWrapper> logger)
    {
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
}
