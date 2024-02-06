using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IAutomationManager
{
    IEnumerable<AutomationWrapper> GetAll();
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
    readonly ISystemObserver _observer;
    private readonly ILogger<AutomationManager> _logger;

    private  Dictionary<Guid, AutomationWrapper> _internalAutomations = new();
    private Dictionary<string, List<AutomationWrapper>> _automationsByTrigger;

    public AutomationManager(
        IEnumerable<IAutomation> automations,
        IEnumerable<IConditionalAutomation> conditionalAutomations,
        IEnumerable<IAutomationRegistry> registries,
        ISystemObserver observer,
        ILogger<AutomationManager> logger)
    {
        _observer = observer;
        this._logger = logger;

        var discoveredConditionals =
            from ca in conditionalAutomations
            let wrapped = new ConditionalAutomationWrapper(ca, _observer, _logger)
            select new AutomationWrapper(wrapped, logger, "Discovered");

        var registeredConditionals = GetRegisteredConditionals(registries);

        var discoverdAutomations = 
            from a in automations
            select new AutomationWrapper(a, logger, "Discovered");

        IEnumerable<AutomationWrapper> registered = GetRegistered(registries);

        _internalAutomations = 
            discoverdAutomations
            .Union(discoveredConditionals)
            .Union(registered)
            .Union(registeredConditionals)
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

    public IEnumerable<AutomationWrapper> GetAll()
    {
        return _internalAutomations.Values;
    }

    public IEnumerable<AutomationWrapper> GetByTriggerEntityId(string entityId)
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

    private IEnumerable<AutomationWrapper> GetRegisteredConditionals(IEnumerable<IAutomationRegistry> registries)
    {
        try
        {
            return 
                from r in registries
                from a in r.RegisterContitionals()
                let conditionalWrapper = new ConditionalAutomationWrapper(a, _observer,_logger)
                select new AutomationWrapper(conditionalWrapper,_logger, r.GetType().Name);
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Registration for conditionals Failed");
            throw;
        }    
    }

    private IEnumerable<AutomationWrapper> GetRegistered(IEnumerable<IAutomationRegistry> registries)
    {
        try
        {
            return 
                from r in registries
                from a in r.Register()
                select new AutomationWrapper(a, _logger, r.GetType().Name);
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Registration Failed");
            throw;
        }
    }

    public bool EnableAutomation(Guid id, bool enable)
    {
        if (_internalAutomations.TryGetValue(id, out var auto))
        {
            auto.GetMetaData().Enabled = enable;
            if (!enable && auto.WrappedAutomation is ConditionalAutomationWrapper conditional)
            {
                //diable any running conditionals
                conditional.StopIfRunning();
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
