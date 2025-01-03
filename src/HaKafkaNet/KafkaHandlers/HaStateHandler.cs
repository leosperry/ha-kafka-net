using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using HaKafkaNet.Implementations.Core;
using KafkaFlow;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IStateHandler
{
    Task Handle(HaEntityState message, CancellationToken cancellationToken = default);
}

internal class HaStateHandler : IMessageHandler<HaEntityState>
{
    private readonly IStateHandler _handler;

    public HaStateHandler(IStateHandler handler, ISystemObserver observer)
    {
        this._handler = handler;
        observer.OnStateHandlerInitialized();
    }

    public async Task Handle(IMessageContext context, HaEntityState message)
    {
        await _handler.Handle(message, context.ConsumerContext.WorkerStopped);
    }
}

class StateHandler : IStateHandler
{
    readonly IDistributedCache _cache;
    readonly IAutomationManager _autoMgr;
    readonly ILogger<HaStateHandler> _logger;

    readonly ISystemObserver _observer;

    readonly HashSet<string> _trackedEntities;

    DateTime _startTime;
    DistributedCacheEntryOptions _cacheOptions = new ()
    {
        SlidingExpiration = TimeSpan.FromDays(30)
    };

    Counter<int> _counter;
    
    public StateHandler(
        IDistributedCache cache, IAutomationManager automationMgr,
        ISystemObserver observer, TimeProvider timeProvider, IAutomationActivator activator, ILogger<HaStateHandler> logger)
    {
        _cache = cache;
        _autoMgr = automationMgr;
        _logger = logger;
        _observer = observer;
        _trackedEntities = automationMgr.GetEntitiesToTrack();
        _startTime = timeProvider.GetLocalNow().LocalDateTime;

        _cacheOptions.SlidingExpiration = TimeSpan.FromDays(30);

        _logger.LogInformation("state handler initialized at :{startTime}", _startTime);

        Meter m = new Meter(Telemetry.MeterStateHandler);
        _counter = m.CreateCounter<int>("ha_kafka_net.message_received_count");

        activator.Activated += state => _ = Task.Run(() => Handle(state, default)); 
    }

    public async Task Handle(HaEntityState message, CancellationToken cancellationToken = default)
    {
        HaEntityState? cached = default;
        
        await Task.WhenAll(
            Task.Run(async () => cached = await HandleCacheAndPrevious(message, cancellationToken)),
            Task.Run(() => _observer.OnEntityStateUpdate(message)),
            Task.Run(() => {
                _counter.Add(1, new KeyValuePair<string, object?>("entity_id", message.EntityId));
                if (_trackedEntities.Contains(message.EntityId) && message.Bad())
                {
                    _observer.OnBadStateDiscovered(new BadEntityState(message.EntityId, message));
                }
            })
        );

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

        _ = Task.Run(() => _autoMgr.TriggerAutomations(stateChange, cancellationToken));
    }

    private async Task<HaEntityState?> HandleCacheAndPrevious(HaEntityState message, CancellationToken cancellationToken)
    {
        var cachedBytes = await _cache.GetAsync(message.EntityId);
        HaEntityState? cached = null;
        if (cachedBytes is not null)
        {
            cached = JsonSerializer.Deserialize<HaEntityState>(cachedBytes)!;
            cached.Previous = null; // don't recursively explode the cache
            message.Previous = cached;
        }

        if (cached is null || message.LastUpdated > cached.LastUpdated)
        {
            // at startup, message could be older than cached
            var value = JsonSerializer.SerializeToUtf8Bytes(message);
            _ = _cache.SetAsync(message.EntityId, value, _cacheOptions, cancellationToken);
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
            _ => EventTiming.PreStartupSameTimeLastCached // extreme edge case
        };
    }
}



