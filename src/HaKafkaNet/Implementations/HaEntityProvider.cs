using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal class HaEntityProvider : IHaEntityProvider
{
    private readonly IHaStateCache _cache;
    private readonly IHaApiProvider _api;
    private readonly ILogger<HaEntityProvider> _logger;

    public HaEntityProvider(IHaStateCache cache, IHaApiProvider api, ILogger<HaEntityProvider> logger)
    {
        this._cache = cache;
        this._api = api;
        this._logger = logger;
    }

    public async Task<HaEntityState?> GetEntityState(string entity_id, CancellationToken cancellationToken = default)
    {
        using (_logger.BeginScope("fetching entity {entity_id}", entity_id))
        {
            try
            {
                var cached = await _cache.Get(entity_id, cancellationToken);
                if (cached is not null)
                {
                    return cached;
                }
                _logger.LogInformation("entity not found in cache"); 
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation(ex, "Error retrieving entity from cache");
            }
            
            var apiReturn = await _api.GetEntityState(entity_id, cancellationToken);
            return apiReturn.entityState;
        }
    }
    public async Task<HaEntityState<T>?> GetEntityState<T>(string entity_id, CancellationToken cancellationToken = default)
    {
        using (_logger.BeginScope("fetching entity {entity_id}", entity_id))
        {
            try
            {
                var cached = await _cache.Get<T>(entity_id, cancellationToken);
                if (cached is not null)
                {
                    return cached;
                }
                _logger.LogInformation("entity not found in cache"); 
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation(ex, "Error retrieving entity from cache");
            }
            
            var apiReturn = await _api.GetEntityState<T>(entity_id, cancellationToken);
            return apiReturn.entityState;
        }
    }
}
