namespace HaKafkaNet;

/// <summary>
/// a collectin of services for working with Home Assistant and entities
/// </summary>
public interface IHaServices
{
    /// <summary>
    /// Provides method for interacting with HA REST API
    /// </summary>
    public IHaApiProvider Api { get; }

    /// <summary>
    /// Provides methods for working your user provided IDistributedCache
    /// </summary>
    public IHaStateCache Cache { get; }

    /// <summary>
    /// Provides methods that attempt to fetch entity state from the cache
    /// and fall back to the HA API
    /// </summary>
    public IHaEntityProvider EntityProvider { get; }
}
