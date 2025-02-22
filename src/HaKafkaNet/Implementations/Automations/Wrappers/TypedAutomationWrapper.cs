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
    public bool IsActive { get => _automation.IsActive; }

    internal readonly IAutomation<Tstate, Tatt> _automation;
    private readonly ISystemObserver _observer;

    private AutomationMetaData? _meta;

    public TypedAutomationWrapper(Tauto automation, ISystemObserver observer)
    {
        this._automation = automation;
        this._observer = observer;
    }

    public override Type WrappedType => _automation.GetType();

    public IAutomationBase WrappedAutomation { get => _automation;}

    public async Task Execute(HaEntityStateChange stateChange, CancellationToken ct)
    {
        HaEntityStateChange<HaEntityState<Tstate,Tatt>> typed;
        try
        {
            typed = stateChange.ToTyped<Tstate, Tatt>();
        }
        catch (System.Exception ex)
        {
            _observer.OnAutomationTypeConversionFailure(ex, this.WrappedAutomation, stateChange, ct);
            if (this.WrappedAutomation is IFallbackExecution fallback)
            {
                await fallback.FallbackExecute(ex, stateChange, ct);
            }
            return;
        }
        
        await _automation.Execute(typed, ct);
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        return _automation.TriggerEntityIds();
    }

    public AutomationMetaData GetMetaData()
    {
        return _meta ??= GetOrMakeMetaData();
    }

    private AutomationMetaData GetOrMakeMetaData()
    {
        IAutomationMeta? autoImplementingMeta = _automation as IAutomationMeta;
        IAutomationBase target = _automation;
        while(autoImplementingMeta is null && target is IAutomationWrapperBase targetWrapper)
        {
            target = targetWrapper.WrappedAutomation;
            autoImplementingMeta = target as IAutomationMeta;
        }

        return autoImplementingMeta is null ? AutomationMetaData.Create(target) : autoImplementingMeta.GetMetaData();
    }
}

