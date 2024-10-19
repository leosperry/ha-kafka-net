using System;

namespace HaKafkaNet;

abstract class TypedAutomationWrapper
{
    public abstract Type WrappedType{ get; }
}

[ExcludeFromDiscovery]
internal class TypedAutomationWrapper<Tstate, Tatt> : TypedAutomationWrapper, IAutomation, IAutomationMeta
{
    public EventTiming EventTimings { get => _automation.EventTimings; }
    internal readonly IAutomation<Tstate, Tatt> _automation;
    
    readonly AutomationMetaData _meta;

    public TypedAutomationWrapper(IAutomation<Tstate, Tatt> automation)
    {
        this._automation = automation;

        if (automation is IAutomationMeta metaAuto)
        {
            _meta = metaAuto.GetMetaData();
            _meta.UnderlyingType = _automation.GetType().Name;
        }
        else
        {
            _meta = AutomationMetaData.Create(this);
        }
    }

    public override Type WrappedType => _automation.GetType();

    public async Task Execute(HaEntityStateChange stateChange, CancellationToken ct)
    {
        var typed = stateChange.ToTyped<Tstate, Tatt>();
        await _automation.Execute(typed, ct);
    }

    public AutomationMetaData GetMetaData() => _meta;

    public IEnumerable<string> TriggerEntityIds()
    {
        return _automation.TriggerEntityIds();
    }
}

