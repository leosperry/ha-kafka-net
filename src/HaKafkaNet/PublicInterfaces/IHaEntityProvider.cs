namespace HaKafkaNet;

/// <summary>
/// Interface for retrieving entities with a strategy of 
/// first retrieving from the cache and using 
/// Home Assistant api as a backup
/// </summary>
public interface IHaEntityProvider
{
    /// <summary>
    /// Retrieves entity state from the cache if exists, otherwise returns from api
    /// </summary>
    /// <param name="entity_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HaEntityState?> GetEntityState(string entity_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves strongly typed entity state from the cache if exists, otherwise returns from api
    /// </summary>
    /// <typeparam name="T">Type to convert attributes</typeparam>
    /// <param name="entity_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HaEntityState<T>?> GetEntityState<T>(string entity_id, CancellationToken cancellationToken = default);
}
