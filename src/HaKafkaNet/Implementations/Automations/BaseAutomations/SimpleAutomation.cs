
namespace HaKafkaNet;

[ExcludeFromDiscovery]
public abstract class SimpleAutomationBase : IAutomation, IAutomationMeta, ISetAutomationMeta
{
    private readonly IEnumerable<string> _triggerEntities;
    private AutomationMetaData? _meta;

    public SimpleAutomationBase(IEnumerable<string> triggerEntities, EventTiming eventTimings)
    {
        this._triggerEntities = triggerEntities;
        this.EventTimings = eventTimings;
    }

    public EventTiming EventTimings { get; protected internal set; }
    public bool IsActive { get; protected internal set; }

    public abstract Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken);

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

    public IEnumerable<string> TriggerEntityIds()
    {
        return _triggerEntities;
    }

    public void SetMeta(AutomationMetaData meta)
    {
        _meta = meta;
    } 
}

[ExcludeFromDiscovery]
public class SimpleAutomation : SimpleAutomationBase
{
    private readonly Func<HaEntityStateChange, CancellationToken, Task> _execute;

    public SimpleAutomation(IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task> execute, EventTiming eventTimings)
        :base(triggerEntities,eventTimings)
    {
        this._execute = execute;
    }

    public override Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        return _execute(stateChange, cancellationToken);
    }
}

[ExcludeFromDiscovery]
public class SimpleAutomationWithServices : SimpleAutomationBase
{
    IHaServices _services;
    private readonly Func<IHaServices, HaEntityStateChange, CancellationToken, Task> _executeWithServices;

    public SimpleAutomationWithServices(IHaServices services, IEnumerable<string> triggerEntities, Func<IHaServices, HaEntityStateChange, CancellationToken, Task> execute, EventTiming eventTimings):
        base(triggerEntities, eventTimings)
    {
        _services = services;
        this._executeWithServices = execute;
    }

    public override Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        return _executeWithServices(_services, stateChange, cancellationToken);
    }
}
