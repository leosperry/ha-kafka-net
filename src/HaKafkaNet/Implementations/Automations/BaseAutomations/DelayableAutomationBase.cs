
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HaKafkaNet;

public abstract class DelayableAutomationBase : IDelayableAutomation
{
    public EventTiming EventTimings { get; protected internal set; } = EventTiming.PostStartup;
    public bool IsActive { get; protected internal set;}

    public bool ShouldExecutePastEvents { get; set; }
    public bool ShouldExecuteOnContinueError { get; set; }

    protected readonly IEnumerable<string> _triggerEntities;

    public DelayableAutomationBase(IEnumerable<string> triggerEntities,
        bool shouldExecutePastEvents = false,
        bool shouldExecuteOnError = false)
    {
        _triggerEntities = triggerEntities;
        ShouldExecutePastEvents = shouldExecutePastEvents;
        ShouldExecuteOnContinueError = shouldExecuteOnError;
    }

    public abstract Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken);

    public abstract Task Execute(CancellationToken cancellationToken);

    public IEnumerable<string> TriggerEntityIds() => _triggerEntities;
}

public abstract class DelayableAutomationBase<Tstate, Tatt> : IDelayableAutomation<Tstate, Tatt> , IAutomationMeta, ISetAutomationMeta
{
    readonly IEnumerable<string> _triggers;
    public EventTiming EventTimings { get; protected internal set; } = EventTiming.PostStartup;
    public bool IsActive { get; protected internal set;}
    public bool ShouldExecutePastEvents { get; set; } = false;
    public bool ShouldExecuteOnContinueError { get; set; } = false;

    public DelayableAutomationBase(IEnumerable<string> triggers)
    {
        _triggers = triggers;
    }

    public abstract Task<bool> ContinuesToBeTrue(HaEntityStateChange<HaEntityState<Tstate, Tatt>> stateChange, CancellationToken ct);

    public abstract Task Execute(CancellationToken ct);

    public IEnumerable<string> TriggerEntityIds() => _triggers;

    private AutomationMetaData? _meta;
    public AutomationMetaData GetMetaData()
    {
        return _meta ??= AutomationMetaData.Create(this);
    }

    public void SetMeta(AutomationMetaData meta)
    {
        _meta = meta;
    }  
}
