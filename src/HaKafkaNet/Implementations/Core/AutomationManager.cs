using System.Text.RegularExpressions;

namespace HaKafkaNet;

internal interface IAutomationManager
{
    IEnumerable<IAutomationWrapper> GetAll();
    IEnumerable<IAutomationRegistry>? Registries { get; }
    bool HasAutomationsForEntity(string entityId);
    Task TriggerAutomations(HaEntityStateChange stateChange, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enables or disables an automation
    /// </summary>
    /// <param name="key">The key of the automation to update</param>
    /// <param name="Enable">true to enable; false to disable</param>
    /// <returns>true if found and updated, otherwise false</returns>
    bool EnableAutomation(string key, bool enable);

    IAutomationWrapper? GetByKey(string key);

    HashSet<string> GetEntitiesToTrack();
    void Initialize(List<InitializationError> errors);
}

internal class AutomationManager : IAutomationManager
{
    public IEnumerable<IAutomationRegistry>? Registries { get; private set; }

    private readonly IInternalRegistrar _registrar;

    private  Dictionary<string, IAutomationWrapper> _internalAutomationsByKey = new();
    private Dictionary<string, List<IAutomationWrapper>> _automationsByTrigger = new();

    public AutomationManager(
        IEnumerable<IAutomationRegistry>? registries,
        IInternalRegistrar registrar)
    {
        _registrar = registrar;
        this.Registries = registries?.ToArray();
    }

    public void Initialize(List<InitializationError> errors)
    {
        foreach (var reg in Registries ?? Enumerable.Empty<IAutomationRegistry>())
        {
            try
            {
                // user code
                reg.Register(_registrar);
            }
            catch (System.Exception ex)
            {
                errors.Add(new($"Error initializing {reg.GetType().Name}", ex));
            }
        }

        var allRegistered = _registrar.Registered.ToArray();
        SetKeys(allRegistered, errors);

        _internalAutomationsByKey = allRegistered.ToDictionary(a => a.GetMetaData().GivenKey);

        //get by trigger
        this._automationsByTrigger = (
            from a in allRegistered
            let triggers = a.TriggerEntityIds() ?? Enumerable.Empty<string>()
            where triggers.Any()
            from t in triggers
            group a by t into autoGroup
            let key = autoGroup.Key
            let collection = autoGroup.ToList()
            select (key, collection)).ToDictionary();
    }

    private void SetKeys(IAutomationWrapper[] allRegistered, List<InitializationError> errors)
    {
        Dictionary<string, int> takenKeys = new();
        foreach (var auto in allRegistered)
        {
            var meta = auto.GetMetaData();

            var cleaned = CleanName(meta.KeyRequest ?? meta.Name);
            if (!takenKeys.TryAdd(cleaned, 1))
            {
                takenKeys[cleaned]++;
            }
            if (takenKeys[cleaned] == 1)
            {
                meta.GivenKey = cleaned;
            }
            else
            {
                meta.GivenKey = string.Concat(cleaned, takenKeys[cleaned]);
            }

            // todo: expose exceptions caught in the wrapper
            if (meta.UserMetaError) errors.Add( new($"error in user metadata", null, auto.WrappedAutomation)); 
            if (meta.UserTriggerError) errors.Add( new($"error in fetching triggers", null, auto.WrappedAutomation));
        }
    }

    private string CleanName(string name)
    {
        Regex stripper = new Regex("[^A-Za-z0-9-]+");
        var stripped = stripper.Replace(name, " ").Trim();
        return stripped.Replace(' ', '_').ToLower();
    }

    public IEnumerable<IAutomationWrapper> GetAll()
    {
        return _internalAutomationsByKey.Values;
    }

    public IAutomationWrapper? GetByKey(string key)
    {
        if (_internalAutomationsByKey.TryGetValue(key, out var wrapper))
        {
            return wrapper;
        }
        return null;
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
        bool checkAgainstMetadata(HaEntityState state, AutomationMetaData meta) => meta.Enabled && (meta.TriggerOnBadState || !state.Bad());

        var tasks = 
            from a in GetByTriggerEntityId(stateChange.EntityId)
            where 
                (a.EventTimings & stateChange.EventTiming) == stateChange.EventTiming
                && checkAgainstMetadata(stateChange.New ,a.GetMetaData())
            select a.Execute(stateChange, cancellationToken);
            
        return Task.WhenAll(tasks.ToArray());
    }

    public bool HasAutomationsForEntity(string entityId)
        => _automationsByTrigger.ContainsKey(entityId);

    public bool EnableAutomation(string key, bool enable)
    {
        if (_internalAutomationsByKey.TryGetValue(key, out var auto))
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

    public HashSet<string> GetEntitiesToTrack()
    {
        var ids = 
            from a in _internalAutomationsByKey.Values
            let meta = a.GetMetaData()
            where meta.Enabled
            let autoIds = 
                a.TriggerEntityIds().Union(
                    meta.AdditionalEntitiesToTrack is not null 
                        ? meta.AdditionalEntitiesToTrack 
                        : Enumerable.Empty<string>())
            from id in autoIds
            select id;

        return ids.Distinct().ToHashSet();
    }
}
