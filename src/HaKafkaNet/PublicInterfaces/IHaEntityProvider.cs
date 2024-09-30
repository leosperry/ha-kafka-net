
namespace HaKafkaNet;

/// <summary>
/// Interface for retrieving entities with a strategy of 
/// first retrieving from the cache and using 
/// Home Assistant api as a backup
/// </summary>
public interface IHaEntityProvider : IEntityStateProvider
{

}


public interface IEntityStateProvider
{
    Task<IHaEntity?> GetEntity(string entityId, CancellationToken cancellationToken = default);
    Task<IHaEntity<Tstate, Tatt>?> GetEntity<Tstate, Tatt>(string entityId, CancellationToken cancellationToken); 
    Task<T?> GetEntity<T>(string entityId, CancellationToken cancellationToken = default) where T : class;
}