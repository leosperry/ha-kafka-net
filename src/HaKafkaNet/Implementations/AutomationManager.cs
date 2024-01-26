using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IAutomationManager
{
    IEnumerable<IAutomation> GetAll();
    IEnumerable<IAutomation> GetByTriggerEntityId(string entityId);
    bool HasAutomationsForEntity(string entityId);
    //IAutomation GetById(Guid id);
    Task TriggerAutomations(HaEntityStateChange stateChange, CancellationToken cancellationToken);
}

internal class AutomationManager : IAutomationManager
{

    private readonly ILogger<AutomationManager> _logger;

    private readonly List<IAutomation> _internalAutomations = new List<IAutomation>();
    private Dictionary<string, List<IAutomation>> _automationsByTrigger = new();

    public AutomationManager(
        IEnumerable<IAutomation> automations,
        IEnumerable<IConditionalAutomation> conditionalAutomations,
        IEnumerable<IAutomationRegistry> registries,
        IAutomationFactory automationFactory,
        ILogger<AutomationManager> logger)
    {
        this._logger = logger;

        Initialize(automations, conditionalAutomations, registries, automationFactory);
    }

    public IEnumerable<IAutomation> GetAll()
    {
        return _internalAutomations;
    }

    private void Initialize(
        IEnumerable<IAutomation> automations, IEnumerable<IConditionalAutomation> conditionalAutomations, 
        IEnumerable<IAutomationRegistry> registries, IAutomationFactory automationFactory)
    {
        var conditionals =
            from ca in conditionalAutomations.Union(GetRegisteredConditionals(registries, automationFactory))
            select new ConditionalAutomationWrapper(ca, _logger);

        IEnumerable<IAutomation> registered = GetRegistered(registries, automationFactory);

        //get all
        _internalAutomations.AddRange( 
            automations
            .Union(conditionals)
            .Union(registered));

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

    public IEnumerable<IAutomation> GetByTriggerEntityId(string entityId)
    {
        if (_automationsByTrigger.TryGetValue(entityId, out var automations))
        {
            return automations;
        }
        return Enumerable.Empty<IAutomation>();
    }

    public Task TriggerAutomations(HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        var asdf = 
            from a in GetByTriggerEntityId(stateChange.EntityId)
            where TimingMatches(a.EventTimings, stateChange.EventTiming)
            select Execute(a, stateChange, cancellationToken);
        
        return Task.WhenAll(asdf);
    }

    public bool HasAutomationsForEntity(string entityId)
        => _automationsByTrigger.ContainsKey(entityId);

    private bool TimingMatches(EventTiming automationTimings, EventTiming stateChangeTiming)
    {
        return (automationTimings & stateChangeTiming) == stateChangeTiming;
    }

    private Task Execute(IAutomation _auto, HaEntityStateChange stateChange, CancellationToken cancellationToken)
    {
        using (_logger.BeginScope("Start [{automationType}] from entity [{triggerEntityId}] with context [{contextId}]", 
            _auto.GetType().Name, stateChange.EntityId, stateChange.New.Context?.ID))
        {
            try
            {
                return _auto.Execute(stateChange, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "automation fault");
                throw;
            }
        }
    }
}
