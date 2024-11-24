using System;
using Microsoft.AspNetCore.Http;

namespace HaKafkaNet;
public static class ApiExtensions
{
    #region Constants
    const string
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


    public static Task<HttpResponseMessage> ButtonPress(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService("button", "press", new { entity_id }, cancellationToken);

// Input Helpers
    public static Task<HttpResponseMessage> InputButtonPress(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService("input_button", "press", new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> InputNumberSet(this IHaApiProvider api, string entity_id, int value, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_NUMBER, SET_VALUE, new { entity_id, value }, cancellationToken);
    public static Task<HttpResponseMessage> InputNumberSet(this IHaApiProvider api, string entity_id, float value, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_NUMBER, SET_VALUE, new { entity_id, value }, cancellationToken);    

    public static Task<HttpResponseMessage> InputDateTimeSetTime(this IHaApiProvider api, string entity_id, TimeSpan value, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_DATETIME, SET_DATETIME, new {entity_id, time = value.ToString(@"hh\:mm\:ss")}, cancellationToken);
    public static Task<HttpResponseMessage> InputDateTimeSetDate(this IHaApiProvider api, string entity_id, DateOnly value, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_DATETIME, SET_DATETIME, new {entity_id, date = value.ToString("yyyy-MM-dd")}, cancellationToken);
    public static Task<HttpResponseMessage> InputDateTimeSetDate(this IHaApiProvider api, string entity_id, DateTime value, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_DATETIME, SET_DATETIME, new {entity_id, date = value.Date.ToString("yyyy-MM-dd")}, cancellationToken);
    public static Task<HttpResponseMessage> InputDateTimeSetDateTime(this IHaApiProvider api, string entity_id, DateTime value, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_DATETIME, SET_DATETIME, new {entity_id, datetime = value.ToString("yyyy-MM-dd HH:mm:ss")}, cancellationToken);

    public static Task<HttpResponseMessage> InputSelect_Select<T>(this IHaApiProvider api, string entity_id, T value, CancellationToken cancellationToken = default)
        where T : System.Enum
        => api.CallService(INPUT_SELECT, "select_option", new{entity_id,option = value.ToString()}, cancellationToken);
    public static Task<HttpResponseMessage> InputSelect_Select(this IHaApiProvider api, string entity_id, string value, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_SELECT, "select_option", new{entity_id,option = value.ToString()}, cancellationToken);
    public static Task<HttpResponseMessage> InputSelect_SelectFirst(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_SELECT, "select_first", new{entity_id}, cancellationToken);
    public static Task<HttpResponseMessage> InputSelect_SelectLast(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_SELECT, "select_last", new{entity_id}, cancellationToken);
    public static Task<HttpResponseMessage> InputSelect_SelectNext(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_SELECT, "select_next", new{entity_id}, cancellationToken);
    public static Task<HttpResponseMessage> InputSelect_SelectPrevious(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(INPUT_SELECT, "select_previous", new{entity_id}, cancellationToken);



    public static Task<HttpResponseMessage> InputTextSet(this IHaApiProvider api, string entity_id, string value, CancellationToken cancellationToken = default)
        => api.CallService("input_text", SET_VALUE, new { entity_id, value }, cancellationToken);

    public static Task<HttpResponseMessage> HaAutomationTrigger(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService("automation", "trigger", new{ entity_id}, cancellationToken);

    public static Task<HttpResponseMessage> HaAutomationReload(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService("automation", "reload", new{ entity_id}, cancellationToken);
    public static Task<HttpResponseMessage> LightToggle(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TOGGLE, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> LightToggle(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TOGGLE, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> LightSetBrightness(this IHaApiProvider api, string entity_id, byte brightness, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_ON, new { entity_id, brightness }, cancellationToken);

    public static Task<HttpResponseMessage> LightSetBrightness(this IHaApiProvider api, IEnumerable<string> entity_id, byte brightness, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_ON, new { entity_id, brightness }, cancellationToken);

    public static Task<HttpResponseMessage> LightSetBrightnessByLabel(this IHaApiProvider api, string label_id, byte brightness, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_ON, new { label_id, brightness }, cancellationToken);

    public static Task<HttpResponseMessage> LightSetBrightnessByLabel(this IHaApiProvider api, IEnumerable<string> label_id, byte brightness, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_ON, new { label_id, brightness }, cancellationToken);

    public static Task<HttpResponseMessage> LightTurnOff(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_OFF, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> LightTurnOff(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_OFF, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> LightTurnOn(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_ON, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> LightTurnOn(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_ON, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> LightTurnOn(this IHaApiProvider api, LightTurnOnModel settings, CancellationToken cancellationToken = default)
        => api.CallService(LIGHT, TURN_ON, settings, cancellationToken);

    public static Task<HttpResponseMessage> LockLock(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LOCK, LOCK, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> LockUnLock(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LOCK, "unlock", new { entity_id }, cancellationToken);
    public static Task<HttpResponseMessage> LockOpen(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(LOCK, "open", new { entity_id }, cancellationToken);
    public static Task<HttpResponseMessage> NotifyGroupOrDevice(this IHaApiProvider api, string groupName, string message, string? title = null, CancellationToken cancellationToken = default)
        => api.CallService(NOTIFY, groupName, new { message, title }, cancellationToken);

    /// <summary>
    /// https://companion.home-assistant.io/docs/notifications/notification-commands
    /// </summary>
    /// <param name="api"></param>
    /// <param name="device"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> NotifyCommand(this IHaApiProvider api, string device, AndroidCommand message, dynamic data, CancellationToken cancellationToken = default)
        => api.CallService(NOTIFY, device, new{ message, data}, cancellationToken);

    public static Task<HttpResponseMessage> MediaPlayerMute(this IHaApiProvider api, string entity_id, bool mute, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "volume_mute", new {
            entity_id,
            is_volume_muted = mute
        }, cancellationToken);
    
    public static Task<HttpResponseMessage> MediaPlayerNextTrack(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "media_next_track", new{ entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> MediaPlayerPause(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "media_pause", new{ entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> MediaPlayerPlay(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "media_play", new{ entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> MediaPlayerPlayOrPause(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "media_play_pause", new{ entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> MediaPlayerPreviousTrack(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "media_previous_track", new{ entity_id }, cancellationToken);    
        
    public static Task<HttpResponseMessage> MediaPlayerStop(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "media_stop", new{ entity_id }, cancellationToken);    
        
    public static Task<HttpResponseMessage> MediaPlayerSetRepeat(this IHaApiProvider api, string entity_id, Repeat mode, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "repeat", new{ 
            entity_id,
            repeat = mode.ToString().ToLower()
            }, cancellationToken);    
        
    public static Task<HttpResponseMessage> MediaPlayerShuffle(this IHaApiProvider api, string entity_id, bool shuffle, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "shuffle_set", new{ entity_id, shuffle }, cancellationToken);    

    public static Task<HttpResponseMessage> MediaPlayerSetVolume(this IHaApiProvider api, string entity_id, float volume_level, CancellationToken cancellationToken = default)
    {
        if (volume_level < 0 || volume_level > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(volume_level), "volume must be between 0 and 1 inclusive");
        }
        return api.CallService(MEDIA_PLAYER, "volume_set", new{ entity_id, volume_level}, cancellationToken);
    }

    public static Task<HttpResponseMessage> MediaPlayerVolumeDown(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "volume_down", new{ entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> MediaPlayerVolumeUp(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(MEDIA_PLAYER, "volume_up", new{ entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> NotifyAlexaMedia(this IHaApiProvider api, string message, string[] targets, CancellationToken cancellationToken = default)
        => api.CallService(NOTIFY, "alexa_media", new { message, target = targets }, cancellationToken);

    public static Task<HttpResponseMessage> PersistentNotification(this IHaApiProvider api, string message, CancellationToken cancellationToken = default)
        => api.CallService(NOTIFY, "persistent_notification", new { message }, cancellationToken);
    
    public static Task<HttpResponseMessage> PersistentNotification(this IHaApiProvider api, string title, string message, CancellationToken cancellationToken = default)
        => api.CallService(NOTIFY, "persistent_notification", new { message, title }, cancellationToken);
    

    public static Task<HttpResponseMessage> PersistentNotificationDetail(this IHaApiProvider api, string message, string? title = null, string? notification_id = null, CancellationToken cancellationToken = default)
        => api.CallService(NOTIFY, "persistent_notification", new { message, title, data = new { notification_id} }, cancellationToken);

    public static Task<HttpResponseMessage> RestartHomeAssistant(this IHaApiProvider api, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, "restart", new { }, cancellationToken);

    public static Task<HttpResponseMessage> RemoteSendCommand(this IHaApiProvider api, string entity_id, string command, CancellationToken cancellationToken = default)
        => api.CallService("remote", "send_command", new { entity_id, command }, cancellationToken);

    public static Task<HttpResponseMessage> Speak(this IHaApiProvider api, string speechEntity, string mediaPlayerEntity, string message, bool cache = true, object? options = null, CancellationToken cancellationToken = default)
        => api.CallService("tts", "speak", new
            { 
                entity_id = speechEntity, 
                media_player_entity_id = mediaPlayerEntity, 
                cache, message, 
                options
            } , cancellationToken);

    public static Task<HttpResponseMessage> SpeakPiper(this IHaApiProvider api, string mediaPlayerEntity, string message, bool cache = true, PiperSettings? options = null, CancellationToken cancellationToken = default)
        => api.CallService("tts", "speak", new
            {
                entity_id = "tts.piper", 
                media_player_entity_id = mediaPlayerEntity,
                cache, message, 
                options
            }, cancellationToken);

    public static Task<HttpResponseMessage> SpeakPiper(this IHaApiProvider api, IEnumerable<string> mediaPlayerEntity, string message, bool cache = true, PiperSettings? options = null, CancellationToken cancellationToken = default)
        => api.CallService("tts", "speak", new
            {
                entity_id = "tts.piper", 
                media_player_entity_id = mediaPlayerEntity, 
                cache, message, 
                options
            }, cancellationToken);

    public static Task<HttpResponseMessage> SwitchTurnOff(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(SWITCH, TURN_OFF, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> SwitchTurnOff(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(SWITCH, TURN_OFF, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> SwitchTurnOn(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(SWITCH, TURN_ON, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> SwitchTurnOn(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(SWITCH, TURN_ON, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> SwitchToggle(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(SWITCH, TOGGLE, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> SwitchToggle(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(SWITCH, TOGGLE, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOn(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_ON, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOn(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_ON, new { entity_id }, cancellationToken);
    
    public static Task<HttpResponseMessage> TurnOnArea(this IHaApiProvider api, string area_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_ON, new { area_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOnAreas(this IHaApiProvider api, IEnumerable<string> area_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_ON, new { area_id }, cancellationToken);
    
    public static Task<HttpResponseMessage> TurnOnByLabel(this IHaApiProvider api, string label_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_ON, new { label_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOnByLabels(this IHaApiProvider api, IEnumerable<string> label_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_ON, new { label_id }, cancellationToken);    

    public static Task<HttpResponseMessage> TurnOff(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_OFF, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOff(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_OFF, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOffArea(this IHaApiProvider api, string area_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_OFF, new { area_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOffAreas(this IHaApiProvider api, IEnumerable<string> area_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_OFF, new { area_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOffByLabel(this IHaApiProvider api, string label_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_OFF, new { label_id }, cancellationToken);

    public static Task<HttpResponseMessage> TurnOffByLabels(this IHaApiProvider api, IEnumerable<string> label_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TURN_OFF, new { label_id }, cancellationToken);

    public static Task<HttpResponseMessage> Toggle(this IHaApiProvider api, string entity_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TOGGLE, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> Toggle(this IHaApiProvider api, IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TOGGLE, new { entity_id }, cancellationToken);

    public static Task<HttpResponseMessage> ToggleArea(this IHaApiProvider api, string area_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TOGGLE, new { area_id }, cancellationToken);

    public static Task<HttpResponseMessage> ToggleAreas(this IHaApiProvider api, IEnumerable<string> area_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TOGGLE, new { area_id }, cancellationToken);

    public static Task<HttpResponseMessage> ToggleByLabel(this IHaApiProvider api, string label_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TOGGLE, new { label_id }, cancellationToken);

    public static Task<HttpResponseMessage> ToggleByLabels(this IHaApiProvider api, IEnumerable<string> label_id, CancellationToken cancellationToken = default)
        => api.CallService(HOME_ASSISTANT, TOGGLE, new { label_id }, cancellationToken);

    public static Task<HttpResponseMessage> ZwaveJs_SetConfigParameter(this IHaApiProvider api, object config, CancellationToken cancellationToken = default)
        => api.CallService("zwave_js", "set_config_parameter", config, cancellationToken);
}
