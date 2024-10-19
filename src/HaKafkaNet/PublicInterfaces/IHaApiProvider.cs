
using Microsoft.AspNetCore.Http;

namespace HaKafkaNet;

public interface IHaApiProvider
{
    /// <summary>
    /// Call most services in Home Assistant
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="service"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> CallService(string domain, string service, object data, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> GetErrorLog(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the state of an entity
    /// </summary>
    /// <param name="entity_id"></param>
    /// <returns>A tuple with the response and the entity. If the response is not 200, entityState will be null</returns>
    [Obsolete("please use GetEntity", true)]
    Task<(HttpResponseMessage response, HaEntityState? entityState)> GetEntityState(string entity_id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the state of an entity with stronly type attributes
    /// </summary>
    /// <typeparam name="T">The type to construct from the attributes</typeparam>
    /// <param name="entity_id"></param>
    /// <returns>A tuple with the response and the entity. If the response is not 200, entityState will be null</returns>
    [Obsolete("please use GetEntity", true)]
    Task<(HttpResponseMessage response, HaEntityState<string, T>? entityState)> GetEntityState<T>(string entity_id, CancellationToken cancellationToken = default);

    Task<(HttpResponseMessage response, HaEntityState? entityState)> GetEntity(string entity_id, CancellationToken cancellationToken = default);
    Task<(HttpResponseMessage response, T? entityState)> GetEntity<T>(string entity_id, CancellationToken cancellationToken = default);

    Task<(HttpResponseMessage? response, bool ApiAvailable)> CheckApi();

    /// <summary>
    /// Warning: This endpoint sets the representation of a device within Home Assistant and will not communicate with the actual device. To communicate with the device, use the CallService method or other extentions methods
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="state"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
    Task<(HttpResponseMessage response, IHaEntity<Tstate, Tatt>? returnedState)> SetState<Tstate, Tatt>(string entityId, Tstate state, Tatt attributes);

}
