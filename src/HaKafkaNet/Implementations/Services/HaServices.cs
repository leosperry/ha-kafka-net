namespace HaKafkaNet;

/// <summary>
/// Collection of services for working with HA and retrieving states
/// </summary>
public class HaServices : IHaServices
{
    /// <summary>
    /// provides methods for calling HA API directly
    /// </summary>
    public IHaApiProvider Api { get; private set; }

    /// <summary>
    /// Provides methods for working with your provided IDistributedCache
    /// </summary>
    public IHaStateCache Cache { get; private set; }

    /// <summary>
    /// Provides entities by first going to cache and falling back to API
    /// </summary>
    public IHaEntityProvider EntityProvider { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="api"></param>
    /// <param name="cache"></param>
    /// <param name="haEntityProvider"></param>
    public HaServices(IHaApiProvider api, IHaStateCache cache, IHaEntityProvider haEntityProvider)
    {
        this.Api = api;
        this.Cache = cache;
        this.EntityProvider = haEntityProvider;
    }
}
