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
    DistributedCacheEntryOptions _cacheOptions = new ()
    {
        SlidingExpiration = TimeSpan.FromDays(30)
    };
    
    public HaStateHandler(
        IDistributedCache cache, IAutomationManager automationMgr,
        ISystemObserver observer, ILogger<HaStateHandler> logger)
    {
        _cache = cache;
        _autoMgr = automationMgr;
        _logger = logger;

        _cacheOptions.SlidingExpiration = TimeSpan.FromDays(30);

        observer.OnStateHandlerInitialized();
        _logger.LogInformation("state handler initialized. _startTime:{startTime}", _startTime);
    }

    public async Task Handle(IMessageContext context, HaEntityState message)
    {
        HaEntityState? cached = await HandleCacheAndPrevious(context, message);

        //if no automations need trigger, return
        if (!_autoMgr.HasAutomationsForEntity(message.EntityId))
        {
            return;
        }

        var timing = getEventTiming(cached, message);

        var stateChange = new HaEntityStateChange()
        {
            EventTiming = timing,
            EntityId = message.EntityId,
            New = message,
            Old = timing == EventTiming.PreStartupSameAsLastCached ? cached?.Previous: cached
        };

        _ = _autoMgr.TriggerAutomations(stateChange, context.ConsumerContext.WorkerStopped);
    }


    private async Task<HaEntityState?> HandleCacheAndPrevious(IMessageContext context, HaEntityState message)
    {
        var cachedBytes = await _cache.GetAsync(message.EntityId);
        HaEntityState? cached = null;
        if (cachedBytes is not null)
        {
            cached = JsonSerializer.Deserialize<HaEntityState>(cachedBytes)!;
            cached.Previous = null; // don't recursivly explode the cache
            message.Previous = cached;
        }

        if (cached is null || message.LastUpdated > cached.LastUpdated)
        {
            // at startup, message could be older than cached
            var value = JsonSerializer.SerializeToUtf8Bytes(message);
            _ = _cache.SetAsync(message.EntityId, value, _cacheOptions, context.ConsumerContext.WorkerStopped);
        }

        return cached;
    }

    EventTiming getEventTiming(HaEntityState? cached, HaEntityState current)
    {
        if (current.LastUpdated >= _startTime)
        {
            return EventTiming.PostStartup;
        }
        if (cached is null)
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
