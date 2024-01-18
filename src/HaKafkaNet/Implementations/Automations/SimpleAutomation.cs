
namespace HaKafkaNet;

[ExcludeFromDiscovery]
internal class SimpleAutomation : IAutomation
{
    private readonly IEnumerable<string> _triggerEntities;
    private readonly Func<HaEntityStateChange, CancellationToken, Task> _execute;
    private readonly EventTiming _eventTimings;

    public SimpleAutomation(IEnumerable<string> triggerEntities, Func<HaEntityStateChange, CancellationToken, Task> execute, EventTiming eventTimings)
    {
        this._triggerEntities = triggerEntities;
        this._execute = execute;
        this._eventTimings = eventTimings;
    }

    public EventTiming EventTimings { get => _eventTimings; }

    public Task Execute(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        return _execute(stateChange, cancellationToken);
    }

    public IEnumerable<string> TriggerEntityIds()
    {
        return _triggerEntities;
    }
}
