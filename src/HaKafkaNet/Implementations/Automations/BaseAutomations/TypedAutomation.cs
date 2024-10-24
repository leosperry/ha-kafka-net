using System;

namespace HaKafkaNet;

[ExcludeFromDiscovery]
public class SimpleAutomation<Tstate, Tatt> : IAutomation<Tstate, Tatt>, IAutomationMeta, ISetAutomationMeta
{
    private AutomationMetaData? _meta;
    private readonly Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task> _execute;
    internal readonly EventTiming _eventTimings;
    private readonly string[] _triggers;

    public SimpleAutomation(IEnumerable<string> triggers, Func<HaEntityStateChange<HaEntityState<Tstate, Tatt>>, CancellationToken, Task> execute, EventTiming eventTimings)
    {
        _triggers = triggers.ToArray();
        _execute = execute;
        _eventTimings = eventTimings;
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
