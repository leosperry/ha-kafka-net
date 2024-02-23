using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IAutomationManager
{
    IEnumerable<IAutomationWrapper> GetAll();
    bool HasAutomationsForEntity(string entityId);
    Task TriggerAutomations(HaEntityStateChange stateChange, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enables or disables an automation
    /// </summary>
    /// <param name="id">The ID of the automation to update</param>
    /// <param name="Enable">true to enable; false to disable</param>
    /// <returns>true if found and updated, otherwise false</returns>
    bool EnableAutomation(Guid id, bool Enable);

    IEnumerable<string> GetAllEntitiesToTrack();
}

internal class AutomationManager : IAutomationManager
{
    private readonly IInternalRegistrar _registrar;
    readonly ISystemObserver _observer;
    private readonly ILogger<AutomationManager> _logger;

    private  Dictionary<Guid, IAutomationWrapper> _internalAutomations = new();
    private Dictionary<string, List<IAutomationWrapper>> _automationsByTrigger;

    public AutomationManager(
        IEnumerable<IAutomationRegistry>? registries,
        IInternalRegistrar registrar,
        ISystemObserver observer,
        ILogger<AutomationManager> logger)
    {
        _registrar = registrar;
        _observer = observer;
        this._logger = logger;

        foreach (var reg in registries ?? Enumerable.Empty<IAutomationRegistry>())
        {
            reg.Register(_registrar);
        }

        _internalAutomations = _registrar.Registered
            .ToDictionary(a => a.GetMetaData().Id);

        //get by trigger
        this._automationsByTrigger = (
            from a in _internalAutomations.Values
            from t in a.TriggerEntityIds() ?? Enumerable.Empty<string>()
            group a by t into autoGroup
            let key = autoGroup.Key
            let collection = autoGroup.ToList()
            select (key, collection)).ToDictionary();
    }

    public IEnumerable<IAutomationWrapper> GetAll()
    {
        return _internalAutomations.Values;
    }

    public IEnumerable<IAutomationWrapper> GetByTriggerEntityId(string entityId)
    {
        if (_automationsByTrigger.TryGetValue(entityId, out var automations))
        {
            return automations;
        }
        return Enumerable.Empty<AutomationWrapper>();
    }

    public Task TriggerAutomations(HaEntityStateChange stateChange, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            from a in GetByTriggerEntityId(stateChange.EntityId)
            where 
                (a.EventTimings & stateChange.EventTiming) == stateChange.EventTiming
                && a.GetMetaData().Enabled
            select a.Execute(stateChange, cancellationToken).ContinueWith(t => {
                if (t.IsFaulted)
                {
                    _observer.OnUnhandledException(a.GetMetaData(), t.Exception!);
                }
            }));
    }

    public bool HasAutomationsForEntity(string entityId)
        => _automationsByTrigger.ContainsKey(entityId);

    public bool EnableAutomation(Guid id, bool enable)
    {
        if (_internalAutomations.TryGetValue(id, out var auto))
        {
            auto.GetMetaData().Enabled = enable;
            if (auto.WrappedAutomation is DelayablelAutomationWrapper conditional)
            {
                if (!enable)
                {
                    //diable any running conditionals
                    conditional.StopIfRunning(StopReason.Disabled);
                }
                // else if (conditional.WrappedConditional is ISchedulableAutomation)
                // {
                //     //known edge case, provide mechanism for rescheduling
                //     //automation was re-enabled via UI and will not reschedule unless trigger entity reports state change
                //     //not likely an issue for sun based automations because of how often "sun.sun" reports
                // }
            }

            return true;
        }
        //doesnt' exist / not found
        return false;
    }

    public IEnumerable<string> GetAllEntitiesToTrack()
    {
        var ids = 
            from a in _internalAutomations.Values
            let meta = a.GetMetaData()
            where meta.Enabled
            let autoIds = 
                a.TriggerEntityIds().Union(
                    meta.AdditionalEntitiesToTrack is not null 
                        ? meta.AdditionalEntitiesToTrack 
                        : Enumerable.Empty<string>())
            from id in autoIds
            select id;

        return ids.Distinct();
    }
}
