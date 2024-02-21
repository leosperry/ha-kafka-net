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

    [Obsolete("", false)]
    public Task<HaEntityState?> GetEntityState(string entity_id, CancellationToken cancellationToken = default)
    {
        return GetEntity(entity_id, cancellationToken);
    }

    [Obsolete("please use GetEntity", false)]
    public Task<HaEntityState<T>?> GetEntityState<T>(string entity_id, CancellationToken cancellationToken = default)
    {
        return GetEntity<HaEntityState<T>>(entity_id, cancellationToken);
    }

    public async Task<HaEntityState?> GetEntity(string entityId, CancellationToken cancellationToken = default)
    {
        using (_logger.BeginScope("fetching entity {entity_id}", entityId))
        {
            try
            {
                var cached = await _cache.GetEntity(entityId, cancellationToken);
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
            
            var apiReturn = await _api.GetEntity(entityId, cancellationToken);
            return apiReturn.entityState;
        }
    }

    public async Task<T?> GetEntity<T>(string entityId, CancellationToken cancellationToken = default) where T : class
    {
        using (_logger.BeginScope("fetching entity {entity_id}", entityId))
        using (_logger.BeginScope("fetching entity {entity_id}", entityId))
        {
            try
            {
                var cached = await _cache.GetEntity<T>(entityId, cancellationToken);
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
            
            var apiReturn = await _api.GetEntity<T>(entityId, cancellationToken);
            
            return apiReturn.entityState;
        }    
    }

    public Task<HaEntityState<Tstate, Tatt>?> GetEntity<Tstate, Tatt>(string entityId, CancellationToken cancellationToken)
    {
        return GetEntity<HaEntityState<Tstate, Tatt>>(entityId, cancellationToken);
    }
}
