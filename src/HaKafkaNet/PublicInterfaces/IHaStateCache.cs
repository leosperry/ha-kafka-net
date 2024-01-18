namespace HaKafkaNet;

public interface IHaStateCache
{
    Task<HaEntityState?> Get(string id, CancellationToken cancellationToken = default);
    Task<HaEntityState<T>?> Get<T>(string id, CancellationToken cancellationToken = default);
}
