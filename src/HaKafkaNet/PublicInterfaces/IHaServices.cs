namespace HaKafkaNet;

public interface IHaServices
{
    public IHaApiProvider Api { get; }
    public IHaStateCache Cache { get; }
    public IHaEntityProvider EntityProvider { get; }
}
