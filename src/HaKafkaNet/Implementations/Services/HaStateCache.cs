using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.Json.Serialization;
using HaKafkaNet.Models.JsonConverters;
using Microsoft.Extensions.Caching.Distributed;

namespace HaKafkaNet;

internal class HaStateCache : IHaStateCache
{   
    IDistributedCache _cache;

    static JsonSerializerOptions _options = GlobalConverters.StandardJsonOptions;

    static ActivitySource _activitySource = new ActivitySource(Telemetry.TraceCacheName);

    Counter<int> _hitCount;
    Counter<int> _missCount;
    
    public HaStateCache(IDistributedCache cache)
    {
        _cache = cache;
        Meter meter = new Meter(Telemetry.MeterCacheName);
        _hitCount = meter.CreateCounter<int>("ha_kafka_net.cache_hit_count");
        _missCount = meter.CreateCounter<int>("ha_kafka_net.cache_miss_count");
    }

    /// <summary>
    /// main method for getting entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public async Task<T?> GetEntity<T>(string entityId, CancellationToken cancellationToken) where T : class
    {
        using (var act = _activitySource.StartActivity("ha_kafka_net.get_entity_from_cache"))
        {
            KeyValuePair<string, object?> entityTag = new KeyValuePair<string, object?>("entity_id",entityId);
            act?.AddTag("entity_id", entityId);
            var cached = await _cache.GetAsync(entityId, cancellationToken);
            if(cached != null)
            {
                _hitCount.Add(1, entityTag);
                return JsonSerializer.Deserialize<T>(cached, _options);
            }
            _missCount.Add(1, entityTag);
            return null;
        }
    }

    public async Task<IHaEntity?> GetEntity(string entityId, CancellationToken cancellationToken = default)
        => await GetEntity<HaEntityState>(entityId, cancellationToken);

    public async Task<IHaEntity<Tstate, Tatt>?> GetEntity<Tstate, Tatt>(string entityId, CancellationToken cancellationToken)
    {
        return await GetEntity<HaEntityState<Tstate, Tatt>>(entityId, cancellationToken);
    }
}
