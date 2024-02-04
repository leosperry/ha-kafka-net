
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

    /// <summary>
    /// Gets the state of an entity
    /// </summary>
    /// <param name="entity_id"></param>
    /// <returns>A tuple with the response and the entity. If the response is not 200, entityState will be null</returns>
    Task<(HttpResponseMessage response, HaEntityState? entityState)> GetEntityState(string entity_id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the state of an entity with stronly type attributes
    /// </summary>
    /// <typeparam name="T">The type to construct from the attributes</typeparam>
    /// <param name="entity_id"></param>
    /// <returns>A tuple with the response and the entity. If the response is not 200, entityState will be null</returns>
    Task<(HttpResponseMessage response, HaEntityState<T>? entityState)> GetEntityState<T>(string entity_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the brightness of a light
    /// </summary>
    /// <param name="entity_id"></param>
    /// <param name="brightness"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> LightSetBrightness(string entity_id, byte brightness = 255, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> LightSetBrightness(IEnumerable<string> entity_id, byte brightness = 255, CancellationToken cancellationToken = default);
    
    Task<HttpResponseMessage> LightToggle(string entity_id, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> LightToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);


    /// <summary>
    /// Turns on a light
    /// </summary>
    /// <param name="entity_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> LightTurnOn(string entity_id, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> LightTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// turns on a light with settings (e.g. color, brightness, etc.)
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> LightTurnOn(LightTurnOnModel settings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Turns off a light
    /// </summary>
    /// <param name="entity_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> LightTurnOff(string entity_id, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> LightTurnOff(IEnumerable<string> entity_ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to a "Notify Group" as documented here:
    /// https://www.home-assistant.io/integrations/group/#notify-groups
    /// Can also send a message to a device
    /// </summary>
    /// <param name="groupName">name of the group or device to notify</param>
    /// <param name="message">message to send</param>
    /// <returns></returns>
    Task<HttpResponseMessage> NotifyGroupOrDevice(string groupName, string message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a message to play on an Alexa
    /// </summary>
    /// <param name="message"></param>
    /// <param name="targets">Names of your Alexa device (e.g. "Office", "Living Room", etc</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> NotifyAlexaMedia(string message, string[] targets, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a persistent notification to Home Assistant
    /// </summary>
    /// <param name="message">contents of the notification</param>
    /// <returns></returns>
    Task<HttpResponseMessage> PersistentNotification(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Turns off a switch
    /// </summary>
    /// <param name="entity_id">id of switch to turn off</param>
    /// <returns></returns>
    Task<HttpResponseMessage> SwitchTurnOff(string entity_id, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> SwitchTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Turns on a switch
    /// </summary>
    /// <param name="entity_id">Entity of switch to turn on</param>
    /// <param name="brightness">Brightness to set</param>
    /// <returns></returns>
    Task<HttpResponseMessage> SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> SwitchTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles a switch
    /// </summary>
    /// <param name="entity_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> SwitchToggle(string entity_id, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> SwitchToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);
}
