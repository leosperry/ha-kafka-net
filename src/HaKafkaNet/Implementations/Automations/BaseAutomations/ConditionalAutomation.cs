
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HaKafkaNet;

[ExcludeFromDiscovery]
public abstract class ConditionalAutomationBase : DelayableAutomationBase, IConditionalAutomationBase, IAutomationMeta, ISetAutomationMeta
{
    readonly TimeSpan _for;
    AutomationMetaData? _meta;

    public ConditionalAutomationBase(IEnumerable<string> triggerEntities, TimeSpan @for): base(triggerEntities)
    {
        this._for = @for;
    }
    public TimeSpan For => _for;

    public virtual AutomationMetaData GetMetaData()
    {
        var thisType = this.GetType();
        return _meta ??= new AutomationMetaData()
        {
            Name = thisType.Name,
            Description = thisType.FullName,
            Enabled = true,
        };
    }

    public void SetMeta(AutomationMetaData meta)
    {
        _meta = meta;
    }
}

[ExcludeFromDiscovery]
public class ConditionalAutomation : ConditionalAutomationBase, IConditionalAutomation
{
    private readonly Func<HaEntityStateChange, CancellationToken, Task<bool>> _continuesToBeTrue;

    private readonly Func<CancellationToken, Task> _execute;

    public ConditionalAutomation(
        IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue, 
        TimeSpan @for, Func<CancellationToken, Task> execute) 
            : base(triggerEntities, @for)
    {
        this._continuesToBeTrue = continuesToBeTrue;
        this._execute = execute;
    }

    public override Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        return _continuesToBeTrue(haEntityStateChange, cancellationToken);
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        return _execute(cancellationToken);
    }
}

[ExcludeFromDiscovery]
public class ConditionalAutomation<Tstate, Tatt> : DelayableAutomationBase<Tstate, Tatt>, IConditionalAutomation<Tstate, Tatt>
{
    private readonly TimeSpan _for;

    private Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task<bool>> _continue;
    private Func<CancellationToken, Task> _execute;

    public ConditionalAutomation(IEnumerable<string> triggers, 
        TimeSpan @for,
        Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task<bool>> continueTrue,
        Func<CancellationToken, Task> execute) : base(triggers)
    {
        _for = @for;
        _continue = continueTrue;
        _execute = execute;
    }

    public TimeSpan For => _for;

    public override async Task<bool> ContinuesToBeTrue(HaEntityStateChange<HaEntityState<Tstate, Tatt>> stateChange, CancellationToken ct)
    {
        return await _continue(stateChange, ct);
    }

    public override async Task Execute(CancellationToken ct)
    {
        await _execute(ct);
    }
}
