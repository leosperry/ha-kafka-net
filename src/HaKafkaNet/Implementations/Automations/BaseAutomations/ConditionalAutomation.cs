
namespace HaKafkaNet;

[ExcludeFromDiscovery]
public abstract class ConditionalAutomationBase : DelayableAutomationBase, IConditionalAutomation, IAutomationMeta, ISetAutomationMeta
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
            UnderlyingType = thisType.Name
        };
    }

    public void SetMeta(AutomationMetaData meta)
    {
        _meta = meta;
    }
}

[ExcludeFromDiscovery]
public class ConditionalAutomation : ConditionalAutomationBase
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
public class ConditionalAutomationWithServices : ConditionalAutomationBase
{
    private readonly IHaServices _services;
    private readonly Func<IHaServices, HaEntityStateChange, CancellationToken, Task<bool>> _continuesToBeTrue;
    private readonly Func<IHaServices, CancellationToken, Task> _execute;

    public ConditionalAutomationWithServices(
        IHaServices services,IEnumerable<string> triggerEntities, 
        Func<IHaServices, HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue, 
        TimeSpan @for, Func<IHaServices, CancellationToken, Task> execute) 
            : base(triggerEntities, @for)
    {
        this._services = services;
        this._continuesToBeTrue = continuesToBeTrue;
        this._execute = execute;
    }

    public override Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken)
    {
        return _continuesToBeTrue(_services, haEntityStateChange, cancellationToken);
    }

    public override Task Execute(CancellationToken cancellationToken)
    {
        return _execute(_services, cancellationToken);
    }
}
