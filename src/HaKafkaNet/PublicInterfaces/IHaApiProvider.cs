
using Microsoft.AspNetCore.Http;

namespace HaKafkaNet;

/// <summary>
/// Provides methods for interacting with HA REST API
/// </summary>
public partial interface IHaApiProvider
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
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetErrorLog(CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(HttpResponseMessage response, HaEntityState? entityState)> GetEntity(string entity_id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity_id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(HttpResponseMessage response, T? entityState)> GetEntity<T>(string entity_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<(HttpResponseMessage? response, bool ApiAvailable)> CheckApi();

    /// <summary>
    /// Warning: This endpoint sets the representation of a device within Home Assistant and will not communicate with the actual device. To communicate with the device, use the CallService method or other extensions methods
    /// </summary>
    /// <typeparam name="Tstate"></typeparam>
    /// <typeparam name="Tatt"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="state"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
    Task<(HttpResponseMessage response, IHaEntity<Tstate, Tatt>? returnedState)> SetState<Tstate, Tatt>(string entityId, Tstate state, Tatt attributes);


    /*
    Below this line is default implementations for various service calls
    These used to be extension methods, but having them here allow mocking frameworks to mock them.
    */
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


    #region Constants
    const string
        CLIMATE = "climate",
        HOME_ASSISTANT = "homeassistant",
        INPUT_DATETIME = "input_datetime",
        INPUT_NUMBER = "input_number",
        INPUT_SELECT = "input_select",
        NOTIFY = "notify",
        TURN_ON = "turn_on",
        TURN_OFF = "turn_off",
        TOGGLE = "toggle",
        LIGHT = "light",
        LOCK = "lock",
        MEDIA_PLAYER = "media_player",
        SET_VALUE = "set_value",
        SET_DATETIME = "set_datetime",
        SWITCH = "switch";
    #endregion

    
    public async Task<HttpResponseMessage> ButtonPress(string entity_id, CancellationToken cancellationToken = default)
        => await CallService("button", "press", new { entity_id }, cancellationToken);

    #region Climate Helpers

    public async Task<HttpResponseMessage> Climate_SetAuxHeat(string entity_id, bool aux_heat, CancellationToken ct = default)
        => await CallService(CLIMATE, "set_aux_heat", new { entity_id, aux_heat }, ct);

    public async Task<HttpResponseMessage> Climate_SetFanMode(string entity_id, string fan_mode, CancellationToken ct = default)
        => await CallService(CLIMATE, "set_fan_mode", new { entity_id, fan_mode }, ct);

    public async Task<HttpResponseMessage> Climate_SetFanMode(string entity_id, CarrierFanMode fan_mode, CancellationToken ct = default)
        => await CallService(CLIMATE, "set_fan_mode", new { entity_id, fan_mode }, ct);

    public async Task<HttpResponseMessage> Climate_SetHumidity(string entity_id, int humidity, CancellationToken ct = default)
        => await CallService(CLIMATE, "set_humidity", new { entity_id, humidity }, ct);

    public async Task<HttpResponseMessage> Climate_SetHvacMode(string entity_id, HvacMode hvac_mode, CancellationToken ct = default)
        => await CallService(CLIMATE, "set_hvac_mode", new { entity_id, hvac_mode }, ct);

    public async Task<HttpResponseMessage> Climate_PresetMode(string entity_id, string preset_mode, CancellationToken ct = default)
        => await CallService(CLIMATE, "set_preset_mode", new { entity_id, preset_mode }, ct);
    
    public async Task<HttpResponseMessage> Climate_PresetMode(string entity_id, CarrierPresetMode preset_mode, CancellationToken ct = default)
        => await CallService(CLIMATE, "set_preset_mode", new { entity_id, preset_mode }, ct);
    
    public async Task<HttpResponseMessage> Climate_SetTemperature(string entity_id, HvacMode? hvac_mode = null, 
        float? target_temp_low = null, float? target_temp_high = null, float? temperature = null, CancellationToken ct = default)
        => await CallService(CLIMATE, "set_temperature", new { entity_id, hvac_mode, target_temp_low, target_temp_high, temperature }, ct);
    
    #endregion

    #region Input Helpers
    public async Task<HttpResponseMessage> InputButtonPress(string entity_id, CancellationToken cancellationToken = default)
        => await CallService("input_button", "press", new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> InputNumberDecrement(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(INPUT_NUMBER, "decrement", new { entity_id }, cancellationToken);
    public async Task<HttpResponseMessage> InputNumberIncrement(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(INPUT_NUMBER, "increment", new { entity_id }, cancellationToken);
    public async Task<HttpResponseMessage> InputNumberSet(string entity_id, int value, CancellationToken cancellationToken = default)
        => await CallService(INPUT_NUMBER, SET_VALUE, new { entity_id, value }, cancellationToken);
    public async Task<HttpResponseMessage> InputNumberSet(string entity_id, float value, CancellationToken cancellationToken = default)
        => await CallService(INPUT_NUMBER, SET_VALUE, new { entity_id, value }, cancellationToken);    

    public async Task<HttpResponseMessage> InputDateTimeSetTime(string entity_id, TimeSpan value, CancellationToken cancellationToken = default)
        => await CallService(INPUT_DATETIME, SET_DATETIME, new {entity_id, time = value.ToString(@"hh\:mm\:ss")}, cancellationToken);
    public async Task<HttpResponseMessage> InputDateTimeSetDate(string entity_id, DateOnly value, CancellationToken cancellationToken = default)
        => await CallService(INPUT_DATETIME, SET_DATETIME, new {entity_id, date = value.ToString("yyyy-MM-dd")}, cancellationToken);
    public async Task<HttpResponseMessage> InputDateTimeSetDate(string entity_id, DateTime value, CancellationToken cancellationToken = default)
        => await CallService(INPUT_DATETIME, SET_DATETIME, new {entity_id, date = value.Date.ToString("yyyy-MM-dd")}, cancellationToken);
    public async Task<HttpResponseMessage> InputDateTimeSetDateTime(string entity_id, DateTime value, CancellationToken cancellationToken = default)
        => await CallService(INPUT_DATETIME, SET_DATETIME, new {entity_id, datetime = value.ToString("yyyy-MM-dd HH:mm:ss")}, cancellationToken);

    public async Task<HttpResponseMessage> InputSelect_Select<T>(string entity_id, T value, CancellationToken cancellationToken = default)
        where T : System.Enum
        => await CallService(INPUT_SELECT, "select_option", new{entity_id,option = value.ToString()}, cancellationToken);
    public async Task<HttpResponseMessage> InputSelect_Select(string entity_id, string value, CancellationToken cancellationToken = default)
        => await CallService(INPUT_SELECT, "select_option", new{entity_id,option = value.ToString()}, cancellationToken);
    public async Task<HttpResponseMessage> InputSelect_SelectFirst(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(INPUT_SELECT, "select_first", new{entity_id}, cancellationToken);
    public async Task<HttpResponseMessage> InputSelect_SelectLast(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(INPUT_SELECT, "select_last", new{entity_id}, cancellationToken);
    public async Task<HttpResponseMessage> InputSelect_SelectNext(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(INPUT_SELECT, "select_next", new{entity_id}, cancellationToken);
    public async Task<HttpResponseMessage> InputSelect_SelectPrevious(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(INPUT_SELECT, "select_previous", new{entity_id}, cancellationToken);

    public async Task<HttpResponseMessage> InputTextSet(string entity_id, string value, CancellationToken cancellationToken = default)
        => await CallService("input_text", SET_VALUE, new { entity_id, value }, cancellationToken);

    #endregion

    public async Task<HttpResponseMessage> HaAutomationTrigger(string entity_id, CancellationToken cancellationToken = default)
        => await CallService("automation", "trigger", new{ entity_id}, cancellationToken);

    public async Task<HttpResponseMessage> HaAutomationReload(string entity_id, CancellationToken cancellationToken = default)
        => await CallService("automation", "reload", new{ entity_id}, cancellationToken);
    public async Task<HttpResponseMessage> LightToggle(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TOGGLE, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> LightToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TOGGLE, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> LightSetBrightness(string entity_id, byte brightness, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_ON, new { entity_id, brightness }, cancellationToken);

    public async Task<HttpResponseMessage> LightSetBrightness(IEnumerable<string> entity_id, byte brightness, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_ON, new { entity_id, brightness }, cancellationToken);

    public async Task<HttpResponseMessage> LightSetBrightnessByLabel(string label_id, byte brightness, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_ON, new { label_id, brightness }, cancellationToken);

    public async Task<HttpResponseMessage> LightSetBrightnessByLabel(IEnumerable<string> label_id, byte brightness, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_ON, new { label_id, brightness }, cancellationToken);

    public async Task<HttpResponseMessage> LightTurnOff(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_OFF, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> LightTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_OFF, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> LightTurnOn(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_ON, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> LightTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_ON, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> LightTurnOn(LightTurnOnModel settings, CancellationToken cancellationToken = default)
        => await CallService(LIGHT, TURN_ON, settings, cancellationToken);

    public async Task<HttpResponseMessage> LockLock(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(LOCK, LOCK, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> LockUnLock(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(LOCK, "unlock", new { entity_id }, cancellationToken);
    public async Task<HttpResponseMessage> LockOpen(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(LOCK, "open", new { entity_id }, cancellationToken);
    public async Task<HttpResponseMessage> NotifyGroupOrDevice(string groupName, string message, string? title = null, CancellationToken cancellationToken = default)
        => await CallService(NOTIFY, groupName, new { message, title }, cancellationToken);

    public async Task<HttpResponseMessage> NotifyDeviceWithActions(string groupName, string message, NotificationAction[] actions, string? title = null, CancellationToken cancellationToken = default)
    {
         var actionsWrapper = new { actions };
        return await CallService(NOTIFY, groupName, new { message, title, data = actionsWrapper }, cancellationToken);
    }
        
    /// <summary>
    /// https://companion.home-assistant.io/docs/notifications/notification-commands
    /// </summary>
    /// <param name="device"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> NotifyCommand(string device, AndroidCommand message, dynamic data, CancellationToken cancellationToken = default)
        => await CallService(NOTIFY, device, new { message, data }, cancellationToken);

    public async Task<HttpResponseMessage> MediaPlayerMute(string entity_id, bool mute, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "volume_mute", new {
            entity_id,
            is_volume_muted = mute
        }, cancellationToken);
    
    public async Task<HttpResponseMessage> MediaPlayerNextTrack(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "media_next_track", new{ entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> MediaPlayerPause(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "media_pause", new{ entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> MediaPlayerPlay(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "media_play", new{ entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> MediaPlayerPlayOrPause(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "media_play_pause", new{ entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> MediaPlayerPreviousTrack(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "media_previous_track", new{ entity_id }, cancellationToken);    
        
    public async Task<HttpResponseMessage> MediaPlayerStop(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "media_stop", new{ entity_id }, cancellationToken);    
        
    public async Task<HttpResponseMessage> MediaPlayerSetRepeat(string entity_id, Repeat mode, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "repeat", new{ 
            entity_id,
            repeat = mode.ToString().ToLower()
            }, cancellationToken);    
        
    public async Task<HttpResponseMessage> MediaPlayerShuffle(string entity_id, bool shuffle, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "shuffle_set", new{ entity_id, shuffle }, cancellationToken);    

    public async Task<HttpResponseMessage> MediaPlayerSetVolume(string entity_id, float volume_level, CancellationToken cancellationToken = default)
    {
        if (volume_level < 0 || volume_level > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(volume_level), "volume must be between 0 and 1 inclusive");
        }
        return await CallService(MEDIA_PLAYER, "volume_set", new{ entity_id, volume_level}, cancellationToken);
    }

    public async Task<HttpResponseMessage> MediaPlayerVolumeDown(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "volume_down", new{ entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> MediaPlayerVolumeUp(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(MEDIA_PLAYER, "volume_up", new{ entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> NotifyAlexaMedia(string message, string[] targets, CancellationToken cancellationToken = default)
        => await CallService(NOTIFY, "alexa_media", new { message, target = targets }, cancellationToken);

    public async Task<HttpResponseMessage> PersistentNotification(string message, CancellationToken cancellationToken = default)
        => await CallService(NOTIFY, "persistent_notification", new { message }, cancellationToken);
    
    public async Task<HttpResponseMessage> PersistentNotification(string title, string message, CancellationToken cancellationToken = default)
        => await CallService(NOTIFY, "persistent_notification", new { message, title }, cancellationToken);

    public async Task<HttpResponseMessage> PersistentNotificationDetail(string message, string? title = null, string? notification_id = null, CancellationToken cancellationToken = default)
        => await CallService(NOTIFY, "persistent_notification", new { message, title, data = new { notification_id} }, cancellationToken);

    public async Task<HttpResponseMessage> RestartHomeAssistant(CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, "restart", new { }, cancellationToken);

    public async Task<HttpResponseMessage> RemoteSendCommand(string entity_id, string command, CancellationToken cancellationToken = default)
        => await CallService("remote", "send_command", new { entity_id, command }, cancellationToken);

    public async Task<HttpResponseMessage> Speak(string speechEntity, string mediaPlayerEntity, string message, bool cache = true, object? options = null, CancellationToken cancellationToken = default)
        => await CallService("tts", "speak", new
            { 
                entity_id = speechEntity, 
                media_player_entity_id = mediaPlayerEntity, 
                cache, message, 
                options
            } , cancellationToken);

    public async Task<HttpResponseMessage> SpeakPiper(string mediaPlayerEntity, string message, bool cache = true, PiperSettings? options = null, CancellationToken cancellationToken = default)
        => await CallService("tts", "speak", new
            {
                entity_id = "tts.piper", 
                media_player_entity_id = mediaPlayerEntity,
                cache, message, 
                options
            }, cancellationToken);

    public async Task<HttpResponseMessage> SpeakPiper(IEnumerable<string> mediaPlayerEntity, string message, bool cache = true, PiperSettings? options = null, CancellationToken cancellationToken = default)
        => await CallService("tts", "speak", new
            {
                entity_id = "tts.piper", 
                media_player_entity_id = mediaPlayerEntity, 
                cache, message, 
                options
            }, cancellationToken);

    public async Task<HttpResponseMessage> SwitchTurnOff(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(SWITCH, TURN_OFF, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> SwitchTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(SWITCH, TURN_OFF, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(SWITCH, TURN_ON, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> SwitchTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(SWITCH, TURN_ON, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> SwitchToggle(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(SWITCH, TOGGLE, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> SwitchToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(SWITCH, TOGGLE, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOn(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_ON, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_ON, new { entity_id }, cancellationToken);
    
    public async Task<HttpResponseMessage> TurnOnArea(string area_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_ON, new { area_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOnAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_ON, new { area_id }, cancellationToken);
    
    public async Task<HttpResponseMessage> TurnOnByLabel(string label_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_ON, new { label_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOnByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_ON, new { label_id }, cancellationToken);    

    public async Task<HttpResponseMessage> TurnOff(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_OFF, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_OFF, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOffArea(string area_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_OFF, new { area_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOffAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_OFF, new { area_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOffByLabel(string label_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_OFF, new { label_id }, cancellationToken);

    public async Task<HttpResponseMessage> TurnOffByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TURN_OFF, new { label_id }, cancellationToken);

    public async Task<HttpResponseMessage> Toggle(string entity_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TOGGLE, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> Toggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TOGGLE, new { entity_id }, cancellationToken);

    public async Task<HttpResponseMessage> ToggleArea(string area_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TOGGLE, new { area_id }, cancellationToken);

    public async Task<HttpResponseMessage> ToggleAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TOGGLE, new { area_id }, cancellationToken);

    public async Task<HttpResponseMessage> ToggleByLabel(string label_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TOGGLE, new { label_id }, cancellationToken);

    public async Task<HttpResponseMessage> ToggleByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default)
        => await CallService(HOME_ASSISTANT, TOGGLE, new { label_id }, cancellationToken);

    public async Task<HttpResponseMessage> ZwaveJs_SetConfigParameter(object config, CancellationToken cancellationToken = default)
        => await CallService("zwave_js", "set_config_parameter", config, cancellationToken);
}
