namespace HaKafkaNet;

public interface IHaStateCache : IEntityStateProvider
{
    /// <summary>
    /// Gets a user saved object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="throwOnDeserializeException">rethrow exception if JsonSerializer.Deserialize<typeparamref name="T"/> throws.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetUserDefinedObject<T>(string key, bool throwOnDeserializeException = false, CancellationToken cancellationToken = default) where T: class;
    
    /// <summary>
    /// Storage for any user defined object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SetUserDefinedObject<T>(string key, T item, CancellationToken cancellationToken = default) where T: class;

    /// <summary>
    /// Gets a user saved item. Useful for things like DateTime, int, etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="throwOnParseException">rethrow exception if T.Parse() throws</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetUserDefinedItem<T>(string key, bool throwOnParseException = false, CancellationToken cancellationToken = default) where T : IParsable<T>;

    /// <summary>
    /// Storage for user defined items. Useful for things like DateTime, int, etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SetUserDefinedItem<T>(string key, T item, CancellationToken cancellationToken = default) where T : IParsable<T>;
}
