using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace HaKafkaNet;

public class HaStateCache : IHaStateCache
{   
    IDistributedCache _cache;

    public HaStateCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<HaEntityState?> Get(string id, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync(id);
        if(cached != null)
        {
            return JsonSerializer.Deserialize<HaEntityState>(cached)!;
        }
        return null;
    }

    public async Task<HaEntityState<T>?> Get<T>(string id, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync(id);
        if(cached != null)
        {
            return JsonSerializer.Deserialize<HaEntityState<T>>(cached)!;
        }
        return null;
    }
}
