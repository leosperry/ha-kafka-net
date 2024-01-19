using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using KafkaFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace HaKafkaNet;

internal class HaStateHandler : IMessageHandler<HaEntityState>
{
    IDistributedCache _cache;

    IEnumerable<AutomationTriggerData> _automationData;

    ILogger<HaStateHandler> _logger;

    DateTime _startTime = DateTime.Now;
    DistributedCacheEntryOptions _cacheOptions = new ();
    
    public HaStateHandler(
        IDistributedCache cache, IAutomationCollector automationCollector,
        ILogger<HaStateHandler> logger)
    {
        _cache = cache;

        var combinedAutomations = automationCollector.GetAll();

        _automationData = 
            (from a in combinedAutomations
            let hashSet = a.TriggerEntityIds().ToHashSet<string>()
            let executor = new Executor(
                (stateChange, cancellationToken)=> new Func<Task?>(() => ExecuteAutomation(stateChange, a, cancellationToken)))
            select new AutomationTriggerData(a, hashSet, a.EventTimings, executor
            ))
            .ToArray();
        
        _logger = logger;
        _logger.LogInformation("state handler initialized. _startTime:{startTime}", _startTime);
    }

    public async Task Handle(IMessageContext context, HaEntityState message)
    {
        var cachedBytes = await _cache.GetAsync(message.EntityId);
        HaEntityState cached = null!;
        if (cachedBytes is not null)
        {
            cached = JsonSerializer.Deserialize<HaEntityState>(cachedBytes)!;
        }

        if (cached is null || message.LastUpdated > cached.LastUpdated)
        {
            // at startup, message could be older than cached
            var value = JsonSerializer.SerializeToUtf8Bytes(message);
            _ = _cache.SetAsync(message.EntityId, value, _cacheOptions, context.ConsumerContext.WorkerStopped);
        }

        //if no automations need trigger, return
        if (!_automationData.Any(a => a.TriggerIds.Contains(message.EntityId)))
        {
            return;
        }

        var stateChange = new HaEntityStateChange()
        {
            EventTiming = message.LastUpdated >= _startTime ? EventTiming.PostStartup : getStartupEventTiming(cached!, message),
            EntityId = message.EntityId,
            New = message,
            Old = cached
        };
        
        var funcs = _automationData.Where(ad => ShouldExecute(stateChange, ad));

        Parallel.ForEach(funcs, auto =>
            _ = Task.Run(auto.Executor(stateChange, context.ConsumerContext.WorkerStopped))
            .ContinueWith(task =>
                _logger.LogError(task.Exception, "Automation faulted")
            , TaskContinuationOptions.OnlyOnFaulted));
    }

    private bool ShouldExecute(HaEntityStateChange stateChange, AutomationTriggerData a)
    {
        return 
            a.TriggerIds.Contains(stateChange.EntityId) // entity match
            && (stateChange.EventTiming & a.EventTiming) == stateChange.EventTiming;
    }

    private Task ExecuteAutomation(HaEntityStateChange stateChange, IAutomation a, CancellationToken cancellationToken)
    {
        using (_logger!.BeginScope("Start [{automationType}] from entity [{triggerEntityId}] with context [{contextId}]", a.GetType().Name, stateChange.EntityId, stateChange.New.Context?.ID))
        {
            return a.Execute(stateChange, cancellationToken);
        }
    }


    EventTiming getStartupEventTiming(HaEntityState cached, HaEntityState current)
    {
        if (cached == null)
        {
            return EventTiming.PreStartupNotCached;
        }
        return current.LastUpdated.CompareTo(cached.LastUpdated) switch
        {
            < 0 => EventTiming.PreStartupPreLastCached,
            > 0 => EventTiming.PreStartupPostLastCached,
            var x when current.Context?.ID == cached.Context?.ID => EventTiming.PreStartupSameAsLastCached,
            _ => EventTiming.PreStartupSameTimeLastCached 
        };
    }

    record AutomationTriggerData(IAutomation Automation, HashSet<string> TriggerIds, EventTiming EventTiming, Executor Executor);
    delegate Func<Task?> Executor(HaEntityStateChange stateChange, CancellationToken cancellationToken);

}

