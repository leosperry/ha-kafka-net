using System;

namespace HaKafkaNet;

abstract class TypedAutomationWrapper
{
    public abstract Type WrappedType{ get; }
}

[ExcludeFromDiscovery]
internal class TypedAutomationWrapper<Tstate, Tatt> : TypedAutomationWrapper, IAutomationWrapper<IAutomation<Tstate, Tatt>>
{
    public EventTiming EventTimings { get => _automation.EventTimings; }
    internal readonly IAutomation<Tstate, Tatt> _automation;
    
    public TypedAutomationWrapper(IAutomation<Tstate, Tatt> automation)
    {
        this._automation = automation;
    }

    public override Type WrappedType => _automation.GetType();

    public IAutomation<Tstate, Tatt> WrappedAutomation { get => _automation;}

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

