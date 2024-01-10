using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using KafkaFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using StackExchange.Redis;

namespace HaKafkaNet;

internal class HaStateHandler : IMessageHandler<HaEntityState>
{
    IDistributedCache _cache;

    IEnumerable<AutomationTriggerData> _automationData;
    DateTime _startTime = DateTime.Now;
    DistributedCacheEntryOptions _cacheOptions = new ();
    
    public HaStateHandler(IDistributedCache cache, IEnumerable<IAutomation> automations)
    {
        _cache = cache;

        _automationData = 
            (from a in automations
            let hashSet = a.TriggerEntityIds().ToHashSet()
            let executor = new Executor(
                (stateChange, cancellationToken)=> new Func<Task?>(() => a.Execute(stateChange, cancellationToken)))
            select new AutomationTriggerData(hashSet, a.EventTimings, executor)).ToArray();
    }

    public async Task Handle(IMessageContext context, HaEntityState message)
    {
        var cachedBytes = await _cache.GetAsync(message.EntityId);
        HaEntityState cached = null!;
        if (cachedBytes != null)
        {
            cached = JsonSerializer.Deserialize<HaEntityState>(cachedBytes)!;
        }

        if (cached == null || message.LastUpdated > cached.LastUpdated)
        {
            // at startup, message could be older than cached
            var value = JsonSerializer.SerializeToUtf8Bytes(message);
            _ = _cache.SetAsync(message.EntityId, value, _cacheOptions);
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

        var funcs =
            from a in _automationData
            where ShouldExecute(stateChange, a)
            select a.Executor(stateChange, context.ConsumerContext.WorkerStopped);
        
        Parallel.ForEach(funcs, excecutor =>
            _ = Task.Run(excecutor)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // minimal error handling,
                    foreach (var ex in task.Exception.InnerExceptions)
                    {
                        Console.WriteLine(ex);
                    }
                    //TODO: allow user to choose logger
                }
            }));
    }

    private bool ShouldExecute(HaEntityStateChange stateChange, AutomationTriggerData a)
    {
        return 
            a.TriggerIds.Contains(stateChange.EntityId) // entity match
            && 
            (
                stateChange.EventTiming == EventTiming.PostStartup // post startup
                ||
                (stateChange.EventTiming & a.EventTiming) == stateChange.EventTiming //pre startup
            );
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

    record AutomationTriggerData(HashSet<string> TriggerIds, EventTiming EventTiming, Executor Executor);
    delegate Func<Task?> Executor(HaEntityStateChange stateChange, CancellationToken cancellationToken);

}

