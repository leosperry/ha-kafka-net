using System;

namespace HaKafkaNet;

public abstract class TypedDelayedAutomationWrapper : IAutomationWrapperBase
{
    public EventTiming EventTimings { get => WrappedAutomation.EventTimings;}
    public abstract IAutomationBase WrappedAutomation { get; }
}

[ExcludeFromDiscovery]
internal class TypedDelayedAutomationWrapper<Tauto, Tstate, Tatt> : TypedDelayedAutomationWrapper, IDelayableAutomation where Tauto: IDelayableAutomation<Tstate, Tatt>
{
    IDelayableAutomation<Tstate, Tatt> _automation;
    private readonly ISystemObserver _observer;

    public TypedDelayedAutomationWrapper(Tauto automation, ISystemObserver observer)
    {
        _automation = automation;
        _observer = observer;
    }

    public override IAutomationBase WrappedAutomation => _automation;

    public async Task<bool> ContinuesToBeTrue(HaEntityStateChange stateChange, CancellationToken ct)
    {
        HaEntityStateChange<HaEntityState<Tstate,Tatt>> typed;
        try
        {
            typed = stateChange.ToTyped<Tstate, Tatt>();
        }
        catch (Exception ex)
        {
            _observer.OnAutomationTypeConversionFailure(ex, this._automation, stateChange, ct);
            if (this._automation is IFallbackExecution fallback)
            {
                await fallback.FallbackExecute(ex, stateChange, ct);
            }
            return false;
        }
        return await _automation.ContinuesToBeTrue(typed, ct);
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
