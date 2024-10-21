using System;

namespace HaKafkaNet;

abstract class TypedAutomationWrapper
{
    public abstract Type WrappedType{ get; }
}

[ExcludeFromDiscovery]
internal class TypedAutomationWrapper<Tauto, Tstate, Tatt> : TypedAutomationWrapper, IAutomationWrapper where Tauto: IAutomation<Tstate, Tatt> 
{
    public EventTiming EventTimings { get => _automation.EventTimings; }
    internal readonly IAutomation<Tstate, Tatt> _automation;
    
    public TypedAutomationWrapper(Tauto automation)
    {
        this._automation = automation;
    }

    public override Type WrappedType => _automation.GetType();

    public IAutomationBase WrappedAutomation { get => _automation;}

    public async Task Execute(HaEntityStateChange stateChange, CancellationToken ct)
    {
        var typed = stateChange.ToTyped<Tstate, Tatt>();
        await _automation.Execute(typed, ct);
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        return _automation.TriggerEntityIds();
    }
}

