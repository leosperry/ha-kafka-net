using System;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
public class SimpleAutomation<Tstate, Tatt> : IAutomation<Tstate, Tatt>, IAutomationMeta, ISetAutomationMeta
{
    private AutomationMetaData? _meta;
    private readonly Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task> _execute;
    public EventTiming EventTimings { get; protected internal set; }
    public bool IsActive { get; protected internal set;}
    private readonly string[] _triggers;

    public SimpleAutomation(IEnumerable<string> triggers, Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task> execute, EventTiming eventTimings)
    {
        _triggers = triggers.ToArray();
        _execute = execute;
        this.EventTimings = eventTimings;
    }

    public async Task Execute(HaEntityStateChange<HaEntityState<Tstate, Tatt>> stateChange, CancellationToken ct)
    {
        await _execute(stateChange, ct);
    }

    public IEnumerable<string> TriggerEntityIds() => _triggers;

    public void SetMeta(AutomationMetaData meta)
    {
        _meta = meta;
    }

    public AutomationMetaData GetMetaData()
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
}
