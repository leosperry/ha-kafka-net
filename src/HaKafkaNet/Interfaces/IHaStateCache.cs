namespace HaKafkaNet;

public interface IHaStateCache
{
    Task<HaEntityState?> Get(string id);
    Task<HaEntityState<T>?> Get<T>(string id);
}
