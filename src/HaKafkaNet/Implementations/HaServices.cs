namespace HaKafkaNet;

public class HaServices : IHaServices
{
    public IHaApiProvider Api { get; private set; }
    public IHaStateCache Cache { get; private set; }

    public HaServices(IHaApiProvider api, IHaStateCache cache)
    {
        this.Api = api;
        this.Cache = cache;
    }
}
