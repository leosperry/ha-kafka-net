using System.Text.Json;
using KafkaFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class HaStateHandler : IMessageHandler<HaEntityState>
{
    readonly IDistributedCache _cache;
    readonly IAutomationManager _autoMgr;
    readonly ILogger<HaStateHandler> _logger;

    DateTime _startTime = DateTime.Now;
    DistributedCacheEntryOptions _cacheOptions = new ();
    
    public HaStateHandler(
        IDistributedCache cache, IAutomationManager automationMgr,
        ISystemObserver observer, ILogger<HaStateHandler> logger)
    {
        _cache = cache;
        _autoMgr = automationMgr;
        _logger = logger;

        observer.OnStateHandlerInitialized();
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
        if (!_autoMgr.HasAutomationsForEntity(message.EntityId))
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
        
        _ = _autoMgr.TriggerAutomations(stateChange, context.ConsumerContext.WorkerStopped);
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
}
