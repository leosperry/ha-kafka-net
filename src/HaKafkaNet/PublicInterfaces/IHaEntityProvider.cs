
namespace HaKafkaNet;

/// <summary>
/// Interface for retrieving entities with a strategy of 
/// first retrieving from the cache and using 
/// Home Assistant api as a backup
/// </summary>
public interface IHaEntityProvider : IEntityStateProvider
{

}

/// <summary>
/// provides methods for retrieving entities
/// </summary>
public interface IEntityStateProvider
{
    /// <summary>
    /// Gets an entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IHaEntity?> GetEntity(string entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a strongly typed entity
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IHaEntity<Tstate, Tatt>?> GetEntity<Tstate, Tatt>(string entityId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets an entity typed as any use defined type 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetEntity<T>(string entityId, CancellationToken cancellationToken = default) where T : class;
}