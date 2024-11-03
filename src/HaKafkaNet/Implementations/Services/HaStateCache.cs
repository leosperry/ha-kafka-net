using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HaKafkaNet.Models.JsonConverters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class HaStateCache : IHaStateCache
{   
    IDistributedCache _cache;

    static JsonSerializerOptions _options = GlobalConverters.StandardJsonOptions;

    static ActivitySource _activitySource = new ActivitySource(Telemetry.TraceCacheName);

    DistributedCacheEntryOptions _cacheOptions = new ()
    {
        SlidingExpiration = TimeSpan.FromDays(30)
    };

    Counter<int> _hitCount;
    Counter<int> _missCount;

    ILogger<HaStateCache> _logger;
    
    public HaStateCache(IDistributedCache cache, ILogger<HaStateCache> logger)
    {
        _cache = cache;
        Meter meter = new Meter(Telemetry.MeterCacheName);
        _hitCount = meter.CreateCounter<int>("ha_kafka_net.cache_hit_count");
        _missCount = meter.CreateCounter<int>("ha_kafka_net.cache_miss_count");
        _logger = logger;
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
            if(cached is not null)
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

    public async Task<T?> GetUserDefinedObject<T>(string key, bool throwOnDeserializeException = false, CancellationToken cancellationToken = default) where T: class
    {
        var cached = await _cache.GetAsync(MakeObjectKey(typeof(T).Name, key), cancellationToken);
        if (cached is not null)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(cached, _options);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "failed cache GetObject for key {cache_key}", MakeObjectKey(typeof(T).Name, key));
                if (throwOnDeserializeException)
                {
                    throw;
                }
                return null;
            }
        }
        return null;
    }

    public async Task<bool> SetUserDefinedObject<T>(string key, T item, CancellationToken cancellationToken = default) where T : class
    {
        var value = JsonSerializer.SerializeToUtf8Bytes(item, _options);
        try
        {
            await _cache.SetAsync(MakeObjectKey(typeof(T).Name, key), value, _cacheOptions, cancellationToken);
            return true;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "failed cache SetObject for key {cache_key}", key);
            return false;
        }
    }

    public async Task<T?> GetUserDefinedItem<T>(string key, bool throwOnParseException = false, CancellationToken cancellationToken = default) where T : IParsable<T>
    {
        var cached = await _cache.GetAsync(MakeItemKey(typeof(T).Name, key), cancellationToken);
        if (cached is not null)
        {
            try
            {
                var str = Encoding.UTF8.GetString(cached);
                return T.Parse(str, null);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "failed cache GetItem for key {cache_key}", key);
                if (throwOnParseException)
                {
                    throw;
                }
                return default;
            }
        }
        return default;
    }

    public async Task<bool> SetUserDefinedItem<T>(string key, T item, CancellationToken cancellationToken = default) where T : IParsable<T>
    {
        try
        {
            var value = Encoding.UTF8.GetBytes(item.ToString()!);
            await _cache.SetAsync(MakeObjectKey(typeof(T).Name, key), value, _cacheOptions, cancellationToken);
            return true;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "failed cache SetItem for key {cache_key}", key);
            return false;
        }
    }

    private string MakeItemKey(string type, string key) => $"HaKafkaNet.Item.{type}.{key}";

    private string MakeObjectKey(string type, string key) => $"HaKafkaNet.Object.{type}.{key}";
}
