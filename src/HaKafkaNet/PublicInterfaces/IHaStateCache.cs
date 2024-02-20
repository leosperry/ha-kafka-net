namespace HaKafkaNet;

public interface IHaStateCache : IEntityStateProvider
{
    [Obsolete("please use GetEntity", false)]
    Task<HaEntityState?> Get(string id, CancellationToken cancellationToken = default);

    [Obsolete("please use GetEntity", false)]
    Task<HaEntityState<T>?> Get<T>(string id, CancellationToken cancellationToken = default);
}
