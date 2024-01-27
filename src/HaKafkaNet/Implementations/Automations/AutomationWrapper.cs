
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
public class AutomationWrapper : IAutomation, IAutomationMeta
{
    readonly IAutomation _auto;
    readonly ILogger _log;

    EventTiming _eventTimings;
    AutomationMetaData _meta;
    IEnumerable<string> _triggers;
   
    public EventTiming EventTimings { get => _eventTimings; }

    public AutomationWrapper(IAutomation automation, ILogger logger)
    {
        _auto = automation;
        _log = logger;
        var underlyingType = automation is ConditionalAutomationWrapper ca ? ca.WrappedConditional.GetType() : automation.GetType();
        _meta = 
            automation is IAutomationMeta metaAuto 
            ? metaAuto.GetMetaData() // called only once
            : new AutomationMetaData()
            {
                Name = underlyingType.Name,
                Description = underlyingType.FullName,
                Enabled = true,
                Id = Guid.NewGuid(),
                UnderlyingType = underlyingType.Name
            };
        _triggers = automation.TriggerEntityIds();
        _eventTimings = automation.EventTimings;
    }

    public Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        using (_log.BeginScope("Start [{automationName}] of Type [{automationType}] from entity [{triggerEntityId}] with context [{contextId}]", 
            _meta.Name, _auto.GetType().Name, stateChange.EntityId, stateChange.New.Context?.ID))
        {
            try
            {
                return _auto.Execute(stateChange, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _log.LogError(ex, "automation fault");
                throw;
            }
        }
    }
    
    public AutomationMetaData GetMetaData()
    {
        return _meta;
    }

    public IEnumerable<string> TriggerEntityIds() => _triggers;
}
