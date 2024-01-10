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
    Task CallService(string domain, string service, object data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a persistent notification to Home Assistant
    /// </summary>
    /// <param name="message">contents of the notification</param>
    /// <returns></returns>
    Task PersistentNotification(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to a "Notify Group" as documented here:
    /// https://www.home-assistant.io/integrations/group/#notify-groups
    /// </summary>
    /// <param name="groupName">name of the group to notify</param>
    /// <param name="message">message to send</param>
    /// <returns></returns>
    Task GroupNotify(string groupName, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Turns off a switch
    /// </summary>
    /// <param name="entity_id">id of switch to turn off</param>
    /// <returns></returns>
    Task SwitchTurnOff(string entity_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Turns on a switch
    /// </summary>
    /// <param name="entity_id">Entity of switch to turn on</param>
    /// <param name="brightness">Brightness to set</param>
    /// <returns></returns>
    Task SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default);

    Task LightSetBrightness(string entity_id, byte brightness = 255, CancellationToken cancellationToken = default);
}
