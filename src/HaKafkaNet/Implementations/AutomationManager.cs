using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IAutomationManager
{
    IEnumerable<AutomationWrapper> GetAll();
    bool HasAutomationsForEntity(string entityId);
    Task TriggerAutomations(HaEntityStateChange stateChange, CancellationToken cancellationToken);
    
    /// <summary>
    /// Enables or disables an automation
    /// </summary>
    /// <param name="id">The ID of the automation to update</param>
    /// <param name="Enable">true to enable; false to disable</param>
    /// <returns>true if found and updated, otherwise false</returns>
    bool EnableAutomation(Guid id, bool Enable);
}

internal class AutomationManager : IAutomationManager
{
    private readonly ILogger<AutomationManager> _logger;

    private  Dictionary<Guid, AutomationWrapper> _internalAutomations = new();
    private Dictionary<string, List<AutomationWrapper>> _automationsByTrigger;

    public AutomationManager(
        IEnumerable<IAutomation> automations,
        IEnumerable<IConditionalAutomation> conditionalAutomations,
        IEnumerable<IAutomationRegistry> registries,
        ILogger<AutomationManager> logger)
    {
        this._logger = logger;

        var conditionals =
            from ca in conditionalAutomations.Union(GetRegisteredConditionals(registries))
            select new ConditionalAutomationWrapper(ca, _logger);

        IEnumerable<IAutomation> registered = GetRegistered(registries);

        _internalAutomations = 
            (from a in automations.Union(conditionals).Union(registered)
            select new AutomationWrapper(a, logger)).ToDictionary(a => a.GetMetaData().Id);

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

    public Task TriggerAutomations(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        return Task.WhenAll(
            from a in GetByTriggerEntityId(stateChange.EntityId)
            where 
                (a.EventTimings & stateChange.EventTiming) == stateChange.EventTiming
                && a.GetMetaData().Enabled
            select a.Execute(stateChange, cancellationToken));
    }

    public bool HasAutomationsForEntity(string entityId)
        => _automationsByTrigger.ContainsKey(entityId);

    private IEnumerable<IConditionalAutomation> GetRegisteredConditionals(IEnumerable<IAutomationRegistry> registries)
    {
        try
        {
            return 
                from r in registries
                from a in r.RegisterContitionals()
                select a;
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Registration for conditionals Failed");
            throw;
        }    
    }

    private IEnumerable<IAutomation> GetRegistered(IEnumerable<IAutomationRegistry> registries)
    {
        try
        {
            return 
                from r in registries
                from a in r.Register()
                select a;
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
            return true;
        }
        //doesnt' exist / not found
        return false;
    }
}
