using System;

namespace HaKafkaNet;

public abstract class TypedDelayedAutomationWrapper : IAutomationWrapperBase
{
    public abstract IAutomationBase WrappedAutomation { get; }
}

[ExcludeFromDiscovery]
public class TypedDelayedAutomationWrapper<Tauto, Tstate, Tatt> : TypedDelayedAutomationWrapper, IDelayableAutomation where Tauto: IDelayableAutomation<Tstate, Tatt>
{
    IDelayableAutomation<Tstate, Tatt> _automation;
    public TypedDelayedAutomationWrapper(Tauto automation)
    {
        _automation = automation;
    }

    public override IAutomationBase WrappedAutomation => _automation;

    public async Task<bool> ContinuesToBeTrue(HaEntityStateChange stateChange, CancellationToken ct)
    {
        var typedChange = stateChange.ToTyped<Tstate, Tatt>();
        return await _automation.ContinuesToBeTrue(typedChange, ct);
    }

    public async Task Execute(CancellationToken ct)
    {
        await _automation.Execute(ct);
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        return _automation.TriggerEntityIds();
    }
}
