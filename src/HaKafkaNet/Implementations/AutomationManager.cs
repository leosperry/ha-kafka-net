using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IAutomationManager
{
    IEnumerable<AutomationWrapper> GetAll();
    //IEnumerable<IAutomation> GetByTriggerEntityId(string entityId);
    bool HasAutomationsForEntity(string entityId);
    //IAutomation GetById(Guid id);
    Task TriggerAutomations(HaEntityStateChange stateChange, CancellationToken cancellationToken);
}

internal class AutomationManager : IAutomationManager
{

    private readonly ILogger<AutomationManager> _logger;

    private readonly List<AutomationWrapper> _internalAutomations = new List<AutomationWrapper>();
    private Dictionary<string, List<AutomationWrapper>> _automationsByTrigger;

    public AutomationManager(
        IEnumerable<IAutomation> automations,
        IEnumerable<IConditionalAutomation> conditionalAutomations,
        IEnumerable<IAutomationRegistry> registries,
        IAutomationFactory automationFactory,
        ILogger<AutomationManager> logger)
    {
        this._logger = logger;

        var conditionals =
            from ca in conditionalAutomations.Union(GetRegisteredConditionals(registries, automationFactory))
            select new ConditionalAutomationWrapper(ca, _logger);

        IEnumerable<IAutomation> registered = GetRegistered(registries, automationFactory);

        _internalAutomations = 
            (from a in automations.Union(conditionals).Union(registered)
            select new AutomationWrapper(a, logger)).ToList();

        //get by trigger
        this._automationsByTrigger = (
            from a in _internalAutomations
            from t in a.TriggerEntityIds() ?? Enumerable.Empty<string>()
            // this is the only place to call TriggerEntityIds
            group a by t into autoGroup
            let key = autoGroup.Key
            let collection = autoGroup.ToList()
            select (key, collection)).ToDictionary();
    }

    public IEnumerable<AutomationWrapper> GetAll()
    {
        return _internalAutomations;
    }

    private IEnumerable<IConditionalAutomation> GetRegisteredConditionals(IEnumerable<IAutomationRegistry> registries, IAutomationFactory automationFactory)
    {
        try
        {
            return 
                from r in registries
                from a in r.RegisterContitionals(automationFactory)
                select a;
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Registration for conditionals Failed");
            throw;
        }    
    }

    private IEnumerable<IAutomation> GetRegistered(IEnumerable<IAutomationRegistry> registries, IAutomationFactory automationFactory)
    {
        try
        {
            return 
                from r in registries
                from a in r.Register(automationFactory)
                select a;
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Registration Failed");
            throw;
        }
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
            where TimingMatches(a.EventTimings, stateChange.EventTiming) && a.GetMetaData().Enabled
            select a.Execute(stateChange, cancellationToken));
    }

    public bool HasAutomationsForEntity(string entityId)
        => _automationsByTrigger.ContainsKey(entityId);

    private bool TimingMatches(EventTiming automationTimings, EventTiming stateChangeTiming)
    {
        return (automationTimings & stateChangeTiming) == stateChangeTiming;
    }
}
