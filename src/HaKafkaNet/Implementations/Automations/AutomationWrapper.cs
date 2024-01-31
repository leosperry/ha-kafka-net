
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
internal class AutomationWrapper : IAutomation, IAutomationMeta
{
    readonly IAutomation _auto;
    readonly ILogger _log;

    EventTiming _eventTimings;
    AutomationMetaData _meta;
    IEnumerable<string> _triggers;
   
    public EventTiming EventTimings { get => _eventTimings; }

    internal IAutomation WrappedAutomation
    {
        get => _auto;
    }

    public AutomationWrapper(IAutomation automation, ILogger logger, string source)
    {
        _auto = automation;
        _log = logger;
        var underlyingType = automation is ConditionalAutomationWrapper ca ? ca.WrappedConditional.GetType() : automation.GetType();
        
        if (automation is IAutomationMeta metaAuto)
        {
            _meta = metaAuto.GetMetaData();
            _meta.UnderlyingType = underlyingType.Name;
        }
        else
        {
            _meta = new AutomationMetaData()
            {
                Name = underlyingType.Name,
                Description = underlyingType.FullName,
                Enabled = true,
                Id = Guid.NewGuid(),
                UnderlyingType = underlyingType.Name
            };
        }
        _meta.Source = source;
        
        _triggers = automation.TriggerEntityIds();
        _eventTimings = automation.EventTimings;
    }

    /// <summary>
    /// this one is asyc becase we want to capture that log scope
    /// </summary>
    /// <param name="stateChange"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        using (_log.BeginScope("Start [{automationName}] of Type [{automationType}] from entity [{triggerEntityId}] with context [{contextId}]", 
            _meta.Name, _auto.GetType().Name, stateChange.EntityId, stateChange.New.Context?.ID))
        {
            try
            {
                await _auto.Execute(stateChange, cancellationToken);
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
