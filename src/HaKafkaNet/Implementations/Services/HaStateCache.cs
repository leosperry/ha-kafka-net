using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace HaKafkaNet;

internal class HaStateCache : IHaStateCache
{   
    IDistributedCache _cache;

    public HaStateCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    [Obsolete("please use GetEntity", false)]
    public async Task<HaEntityState?> Get(string id, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync(id);
        if(cached != null)
        {
            return JsonSerializer.Deserialize<HaEntityState>(cached)!;
        }
        return null;
    }

    [Obsolete("please use GetEntity", false)]
    public async Task<HaEntityState<T>?> Get<T>(string id, CancellationToken cancellationToken = default) 
    {
        var cached = await _cache.GetAsync(id);
        if(cached != null)
        {
            return JsonSerializer.Deserialize<HaEntityState<T>>(cached)!;
        }
        return null;
    }

    /// <summary>
    /// main method for getting entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public async Task<T?> GetEntity<T>(string entityId, CancellationToken cancellationToken) where T : class
    {
        var cached = await _cache.GetAsync(entityId, cancellationToken);
        if(cached != null)
        {
            return JsonSerializer.Deserialize<T>(cached);
        }
        return null;     
    }

    public Task<HaEntityState?> GetEntity(string entityId, CancellationToken cancellationToken = default)
        => GetEntity<HaEntityState>(entityId, cancellationToken);

    public Task<HaEntityState<Tstate, Tatt>?> GetEntity<Tstate, Tatt>(string entityId, CancellationToken cancellationToken)
    {
        return GetEntity<HaEntityState<Tstate, Tatt>>(entityId, cancellationToken);
    }
}
