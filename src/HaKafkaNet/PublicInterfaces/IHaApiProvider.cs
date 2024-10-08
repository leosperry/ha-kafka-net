﻿
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

    // Task<HttpResponseMessage> ButtonPress(string entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> InputButtonPress(string entity_id, CancellationToken cancellationToken = default);

    // Task<HttpResponseMessage> HaAutomationTrigger(string entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> HaAutomationReload(string entity_id, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Sets the brightness of a light
    // /// </summary>
    // /// <param name="entity_id"></param>
    // /// <param name="brightness"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // Task<HttpResponseMessage> LightSetBrightness(string entity_id, byte brightness = 255, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> LightSetBrightness(IEnumerable<string> entity_id, byte brightness = 255, CancellationToken cancellationToken = default);
    
    // [Obsolete("Please use generic Toggle()", false)]
    // Task<HttpResponseMessage> LightToggle(string entity_id, CancellationToken cancellationToken = default);
    
    // [Obsolete("Please use generic Toggle()", false)]
    // Task<HttpResponseMessage> LightToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);

    // Task<HttpResponseMessage> LockLock(string entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> LockUnLock(string entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> LockOpen(string entity_id, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Turns on a light
    // /// </summary>
    // /// <param name="entity_id"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // [Obsolete("Please use generic TurnOn()", false)]
    // Task<HttpResponseMessage> LightTurnOn(string entity_id, CancellationToken cancellationToken = default);
    
    // [Obsolete("Please use generic TurnOn()", false)]

    // Task<HttpResponseMessage> LightTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);
    
    // /// <summary>
    // /// turns on a light with settings (e.g. color, brightness, etc.)
    // /// </summary>
    // /// <param name="settings"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // Task<HttpResponseMessage> LightTurnOn(LightTurnOnModel settings, CancellationToken cancellationToken = default);
    
    // /// <summary>
    // /// Turns off a light
    // /// </summary>
    // /// <param name="entity_id"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // [Obsolete("Please use generic TurnOff()", false)]
    // Task<HttpResponseMessage> LightTurnOff(string entity_id, CancellationToken cancellationToken = default);
    
    // [Obsolete("Please use generic TurnOff()", false)]
    // Task<HttpResponseMessage> LightTurnOff(IEnumerable<string> entity_ids, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Sets the volume of a media player
    // /// </summary>
    // /// <param name="entity_id">id of the media player</param>
    // /// <param name="volume_level">must be a value between 0 and 1 Example: 0.15 is 15%</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // Task<HttpResponseMessage> MediaPlayerSetVolume(string entity_id, float volume_level, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> MediaPlayerMute(string entity_id, bool mute, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Sends a notification to a "Notify Group" as documented here:
    // /// https://www.home-assistant.io/integrations/group/#notify-groups
    // /// Can also send a message to a device
    // /// </summary>
    // /// <param name="groupName">name of the group or device to notify</param>
    // /// <param name="message">message to send</param>
    // /// <returns></returns>
    // Task<HttpResponseMessage> NotifyGroupOrDevice(string groupName, string message, CancellationToken cancellationToken = default);
    
    // /// <summary>
    // /// Sends a message to play on an Alexa
    // /// </summary>
    // /// <param name="message"></param>
    // /// <param name="targets">Names of your Alexa device (e.g. "Office", "Living Room", etc</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // Task<HttpResponseMessage> NotifyAlexaMedia(string message, string[] targets, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Sends a persistent notification to Home Assistant
    // /// </summary>
    // /// <param name="message">contents of the notification</param>
    // /// <returns></returns>
    // Task<HttpResponseMessage> PersistentNotification(string message, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> PersistentNotificationDetail(string message, string? title = null, string? notification_id = null, CancellationToken cancellationToken = default);


    // /// <summary>
    // /// Does what's on the tin
    // /// </summary>
    // /// <returns></returns>
    // Task<HttpResponseMessage> RestartHomeAssistant(CancellationToken cancellationToken = default);

    // Task<HttpResponseMessage> RemoteSendCommand(string entity_id, string command, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Text to speach
    // /// </summary>
    // /// <param name="speachEntity">The speech engine entity. This will be something like "tts.piper"</param>
    // /// <param name="mediaPlayerEntity">The media player where you want the speech heard. This will be something like "media_player.kitchen"</param>
    // /// <param name="message">The message you want spoken</param>
    // /// <param name="cache"></param>
    // /// <returns></returns>
    // Task<HttpResponseMessage> Speak(string speechEntity, string mediaPlayerEntity, string message, bool cache = true, object? options = null, CancellationToken cancellationToken = default);
    
    // Task<HttpResponseMessage> SpeakPiper(string mediaPlayerEntity, string message, bool cache = true, PiperSettings? options = default, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> SpeakPiper(IEnumerable<string> mediaPlayerEntity, string message, bool cache = true, PiperSettings? options = null, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Turns off a switch
    // /// </summary>
    // /// <param name="entity_id">id of switch to turn off</param>
    // /// <returns></returns>
    // [Obsolete("Please use generic TurnOff()", false)]
    // Task<HttpResponseMessage> SwitchTurnOff(string entity_id, CancellationToken cancellationToken = default);

    // [Obsolete("Please use generic TurnOff()", false)]
    // Task<HttpResponseMessage> SwitchTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Turns on a switch
    // /// </summary>
    // /// <param name="entity_id">Entity of switch to turn on</param>
    // /// <param name="brightness">Brightness to set</param>
    // /// <returns></returns>
    // [Obsolete("Please use generic TurnOn()", false)]
    // Task<HttpResponseMessage> SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default);

    // [Obsolete("Please use generic TurnOn()", false)]
    // Task<HttpResponseMessage> SwitchTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Toggles a switch
    // /// </summary>
    // /// <param name="entity_id"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // [Obsolete("Please use generic Toggle()", false)]
    // Task<HttpResponseMessage> SwitchToggle(string entity_id, CancellationToken cancellationToken = default);

    // [Obsolete("Please use generic Toggle()", false)]
    // Task<HttpResponseMessage> SwitchToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);

    // Task<HttpResponseMessage> TurnOn(string entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOnArea(string area_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOnAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOnByLabel(string label_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOnByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOff(string entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOffArea(string area_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOffAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOffByLabel(string label_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> TurnOffByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> Toggle(string entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> Toggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> ToggleArea(string area_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> ToggleAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> ToggleByLabel(string label_id, CancellationToken cancellationToken = default);
    // Task<HttpResponseMessage> ToggleByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default);

    // /// <summary>
    // /// Beta functionality: May break in future versions
    // /// </summary>
    // /// <param name="entity_id"></param>
    // /// <param name="config"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // [Obsolete("Not obsolete, but likly to change.", false)]
    // Task<HttpResponseMessage> ZwaveJs_SetConfigParameter(object config, CancellationToken cancellationToken = default);
}
