
namespace HaKafkaNet;

[ExcludeFromDiscovery]
public abstract class ConditionalAutomationBase : IConditionalAutomation, IAutomationMeta
{
    readonly IEnumerable<string> _triggerEntities;
    readonly TimeSpan _for;
    AutomationMetaData? _meta;

    public ConditionalAutomationBase(IEnumerable<string> triggerEntities, TimeSpan @for)
    {
        this._triggerEntities = triggerEntities;
        this._for = @for;
    }
    public TimeSpan For => _for;

    public abstract Task<bool> ContinuesToBeTrue(HaEntityStateChange haEntityStateChange, CancellationToken cancellationToken);

    public abstract Task Execute(CancellationToken cancellationToken);

    public AutomationMetaData GetMetaData()
    {
        var thisType = this.GetType();
        return _meta ??= new AutomationMetaData()
        {
            Name = thisType.Name,
            Description = thisType.FullName,
            Enabled = true,
            Id = Guid.NewGuid(),
            UnderlyingType = thisType.Name
        };
    }
    public IEnumerable<string> TriggerEntityIds()
    {
        return _triggerEntities;
    }

    internal void SetMeta(AutomationMetaData meta)
    {
        _meta = meta;
    }

}

[ExcludeFromDiscovery]
public class ConditionalAutomation : ConditionalAutomationBase
{
    private readonly IEnumerable<string> _triggerEntities;
    private readonly Func<HaEntityStateChange, CancellationToken, Task<bool>> _continuesToBeTrue;
    private readonly TimeSpan _for;
    private readonly Func<CancellationToken, Task> _execute;

    public ConditionalAutomation(
        IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task<bool>> continuesToBeTrue, 
        TimeSpan @for, Func<CancellationToken, Task> execute) 
        : base(triggerEntities, @for)
    {
        this._triggerEntities = triggerEntities;
        this._continuesToBeTrue = continuesToBeTrue;
        this._for = @for;
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
        TimeSpan @for, Func<IHaServices, CancellationToken, Task> execute, AutomationMetaData meta
    ) : base(triggerEntities, @for)
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
