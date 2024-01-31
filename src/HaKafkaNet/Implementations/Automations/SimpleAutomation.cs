
namespace HaKafkaNet;

[ExcludeFromDiscovery]
public abstract class SimpleAutomationBase : IAutomation, IAutomationMeta
{
    private readonly IEnumerable<string> _triggerEntities;
    private readonly EventTiming _eventTimings;
    private AutomationMetaData? _meta;

    public SimpleAutomationBase(IEnumerable<string> triggerEntities, EventTiming eventTimings)
    {
        this._triggerEntities = triggerEntities;
        this._eventTimings = eventTimings;
    }

    public EventTiming EventTimings { get => _eventTimings; }

    public abstract Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken);

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
public class SimpleAutomation : SimpleAutomationBase
{
    private readonly Func<HaEntityStateChange, CancellationToken, Task> _execute;
    private readonly EventTiming _eventTimings;

    public SimpleAutomation(IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task> execute, EventTiming eventTimings)
        :base(triggerEntities,eventTimings)
    {
        this._execute = execute;
        this._eventTimings = eventTimings;
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
