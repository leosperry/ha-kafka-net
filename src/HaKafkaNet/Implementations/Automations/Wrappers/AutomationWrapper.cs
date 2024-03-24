
namespace HaKafkaNet;


[ExcludeFromDiscovery]
internal class AutomationWrapper : IAutomationWrapper
{
    readonly IAutomation _auto;
    readonly IAutomationTraceProvider _trace;
    EventTiming _eventTimings;
    AutomationMetaData _meta;
    IEnumerable<string> _triggers;
   
    public EventTiming EventTimings { get => _eventTimings; }

    public IAutomation WrappedAutomation
    {
        get => _auto;
    }

    public AutomationWrapper(IAutomation automation, IAutomationTraceProvider traceProvider, string source)
    {
        _auto = automation;
        _trace = traceProvider;
        var underlyingType = automation is DelayablelAutomationWrapper ca ? ca.WrappedConditional.GetType() : automation.GetType();
        
        _triggers = automation.TriggerEntityIds();
        _eventTimings = automation.EventTimings;

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
                KeyRequest = GenerateKey(source, underlyingType.Name),
                UnderlyingType = underlyingType.Name
            };
        }

        if (string.IsNullOrEmpty(_meta.KeyRequest))
        {
            _meta.KeyRequest = _meta.Name;
        }

        _meta.Source = source;
        
    }

    /// <summary>
    /// this one is asyc becase we want to capture that log scope
    /// </summary>
    /// <param name="stateChange"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        this._meta.LastTriggered = DateTime.Now;
        TraceEvent evt = new()
        {
            EventType = "Trigger",
            AutomationKey = this._meta.GivenKey,
            EventTime = DateTime.Now,
            StateChange = stateChange,
        };

        await _trace.Trace(evt, _meta, () => _auto.Execute(stateChange, cancellationToken));
    }

    private string GenerateKey(string source, string name)
    {
        var triggers = _triggers.Any() ? _triggers.Aggregate((s1,s2) => $"{s1}-{s2}") : string.Empty;
        return $"{source}-{name}-{triggers}";
    }    

    public AutomationMetaData GetMetaData()
    {
        return _meta;
    }

    public IEnumerable<string> TriggerEntityIds() => _triggers;
}
