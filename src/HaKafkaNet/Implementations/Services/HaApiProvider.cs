﻿using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HaKafkaNet;

internal class HaApiProvider : IHaApiProvider
{
    readonly HttpClient _client;
    readonly HomeAssistantConnectionInfo _apiConfig;
    readonly ISystemObserver _observer;
    readonly ILogger<HaApiProvider> _logger;
    readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new RgbConverter(),
            new RgbwConverter(),
            new RgbwwConverter(),
            new XyConverter(),
            new HsConverter(),
        }
    };

    #region Constants
    const string
        HOME_ASSISTANT = "homeassistant",
        NOTIFY = "notify",
        TURN_ON = "turn_on",
        TURN_OFF = "turn_off",
        TOGGLE = "toggle",
        LIGHT = "light",
        LOCK = "lock",
        SWITCH = "switch";
    #endregion

    static ActivitySource _activitySource = new ActivitySource(Telemetry.TraceApiName);

    public HaApiProvider(IHttpClientFactory clientFactory, HomeAssistantConnectionInfo config, ISystemObserver observer, ILogger<HaApiProvider> logger)
    {
        _client = clientFactory.CreateClient();
        _apiConfig = config;
        _observer = observer;

        _client.BaseAddress = new Uri(config.BaseUri);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiConfig.AccessToken);

        _logger = logger;
    }

    public async Task<HttpResponseMessage> CallService(string domain, string service, object data, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> scope = new()
        {
            {"HaApi.endpoint", "services"},
            {"HaApi.domain" , domain},
            {"HaApi.service", service},
            {"HaApi.data", data}
        };
        using (_logger.BeginScope(scope))
        using (StringContent json = new StringContent(JsonSerializer.Serialize(data, _options)))
        using(var act = _activitySource.StartActivity("ha_kafka_net.ha_api_post"))
        {
            act?.AddTag("ha_domain", domain);
            act?.AddTag("ha_service", service);
            _logger.LogDebug("Calling Home Assistant Service API");
            HttpResponseMessage? response = default;
            try
            {
                response = await _client.PostAsync($"/api/services/{domain}/{service}", json, cancellationToken);

                var status = (int)response.StatusCode;
                if (status < 200 || status >= 400)
                {
                    _observer.OnHaServiceBadResponse(new(domain, service, data, response, default), cancellationToken);
                    _logger.LogWarning("Home Assistant API returned {status}:{reason} \n{content}", response.StatusCode, response.ReasonPhrase, await response.Content.ReadAsStringAsync());
                }
                return response;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException) 
            {
                _logger.LogDebug(ex, "Task wass canceled while calling Home Assistant API");
                throw;
            }
            catch (System.Exception ex)
            {
                _observer.OnHaServiceBadResponse(new(domain, service, data, response, ex), cancellationToken);
                _logger.LogError(ex, "Error calling Home Assistant API: " + ex.Message);
                throw;
            }
        }
    }

    public async Task<HttpResponseMessage> GetErrorLog(CancellationToken cancellationToken = default)
    {
        using(_activitySource.StartActivity("ha_kafka_net.ha_api_error_log"))

        _logger.LogDebug("Calling Home Assistant error log API");
        var response = await _client.GetAsync("/api/error_log", cancellationToken);

        int status = (int)response.StatusCode;
        if (status < 200 || status >= 400)
        {
            _logger.LogWarning("Home Assistant API returned {status}:{reason} \n{content}", response.StatusCode, response.ReasonPhrase, await response.Content.ReadAsStringAsync());
        }
        return response;
    }

    public Task<(HttpResponseMessage response, HaEntityState? entityState)> GetEntityState(string entity_id, CancellationToken cancellationToken = default)
    {
        return GetEntity(entity_id, cancellationToken);
    }

    public Task<(HttpResponseMessage response, HaEntityState<string, T>? entityState)> GetEntityState<T>(string entity_id, CancellationToken cancellationToken = default)
    {
        return GetEntity<HaEntityState<string, T>>(entity_id, cancellationToken);
    }

    public Task<(HttpResponseMessage response, HaEntityState? entityState)> GetEntity(string entity_id, CancellationToken cancellationToken = default)
    {
        return GetEntity<HaEntityState>(entity_id, cancellationToken);
    }

    public async Task<(HttpResponseMessage response, T? entityState)> GetEntity<T>(string entity_id, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> scope = new()
        {
            {"HaApi.entity_id" , entity_id},
            {"HaApi.endpoint" , "states"},
        };
        using(var act = _activitySource.StartActivity("ha_kafka_net.ha_api_get"))
        using (_logger.BeginScope(scope))
        {
            act?.AddTag("entity_id", entity_id);
            _logger.LogTrace("Calling Home Assistant States API");
            var response = await _client.GetAsync($"/api/states/{entity_id}", cancellationToken);

            int status = (int)response.StatusCode;
            if (status >= 400)
            {
                _logger.LogWarning("Home Assistant API returned {status_code}:{reason} \n{content}", response.StatusCode, response.ReasonPhrase, await response.Content.ReadAsStringAsync());
            }
            else
            {
                _logger.LogInformation("Home Assistant api response {status_code}", response.StatusCode);
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                return (response, JsonSerializer.Deserialize<T>(response.Content.ReadAsStream(), _options)!);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                _logger.LogDebug(ex, "Task wass canceled while calling Home Assistant API");
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "could not parse HA API response");
            }
            return (response, default(T?));
        }
    }

    // public Task<HttpResponseMessage> ButtonPress(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService("button", "press", new { entity_id }, cancellationToken);
    // public Task<HttpResponseMessage> InputButtonPress(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService("input_button", "press", new { entity_id }, cancellationToken);


    // public Task<HttpResponseMessage> HaAutomationTrigger(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService("automation", "trigger", new{ entity_id}, cancellationToken);

    // public Task<HttpResponseMessage> HaAutomationReload(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService("automation", "reload", new{ entity_id}, cancellationToken);
    // public Task<HttpResponseMessage> LightToggle(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TOGGLE, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> LightToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TOGGLE, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> LightSetBrightness(string entity_id, byte brightness, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TURN_ON, new { entity_id, brightness }, cancellationToken);

    // public Task<HttpResponseMessage> LightSetBrightness(IEnumerable<string> entity_id, byte brightness, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TURN_ON, new { entity_id, brightness }, cancellationToken);

    // public Task<HttpResponseMessage> LightTurnOff(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TURN_OFF, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> LightTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TURN_OFF, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> LightTurnOn(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TURN_ON, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> LightTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TURN_ON, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> LightTurnOn(LightTurnOnModel settings, CancellationToken cancellationToken = default)
    //     => CallService(LIGHT, TURN_ON, settings, cancellationToken);

    // public Task<HttpResponseMessage> LockLock(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LOCK, LOCK, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> LockUnLock(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LOCK, "unlock", new { entity_id }, cancellationToken);
    // public Task<HttpResponseMessage> LockOpen(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(LOCK, "open", new { entity_id }, cancellationToken);
    // public Task<HttpResponseMessage> NotifyGroupOrDevice(string groupName, string message, CancellationToken cancellationToken = default)
    //     => CallService(NOTIFY, groupName, new { message }, cancellationToken);

    // public Task<HttpResponseMessage> MediaPlayerSetVolume(string entity_id, float volume_level, CancellationToken cancellationToken = default)
    // {
    //     if (volume_level < 0 || volume_level > 1)
    //     {
    //         throw new ArgumentOutOfRangeException(nameof(volume_level), "volume must be between 0 and 1");
    //     }
    //     return CallService("media_player", "volume_set", new{ entity_id, volume_level});
    // }

    // public Task<HttpResponseMessage> MediaPlayerMute(string entity_id, bool mute, CancellationToken cancellationToken = default)
    // {
    //     return CallService("media_player", "volume_mute", new {
    //         entity_id,
    //         is_volume_muted = mute
    //     });
    // }


    // public Task<HttpResponseMessage> NotifyAlexaMedia(string message, string[] targets, CancellationToken cancellationToken = default)
    //     => CallService(NOTIFY, "alexa_media", new { message, target = targets }, cancellationToken);

    // public Task<HttpResponseMessage> PersistentNotification(string message, CancellationToken cancellationToken = default)
    //     => CallService(NOTIFY, "persistent_notification", new { message }, cancellationToken);

    // public Task<HttpResponseMessage> PersistentNotificationDetail(string message, string? title = null, string? notification_id = null, CancellationToken cancellationToken = default)
    //     => CallService(NOTIFY, "persistent_notification", new { message, title, data = new { notification_id} }, cancellationToken);

    // public Task<HttpResponseMessage> RestartHomeAssistant(CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, "restart", new { }, cancellationToken);

    // public Task<HttpResponseMessage> RemoteSendCommand(string entity_id, string command, CancellationToken cancellationToken = default)
    //     => CallService("remote", "send_command", new { entity_id, command }, cancellationToken);

    // public Task<HttpResponseMessage> Speak(string speechEntity, string mediaPlayerEntity, string message, bool cache = true, object? options = null, CancellationToken cancellationToken = default)
    //     => CallService("tts", "speak", new
    //         { 
    //             entity_id = speechEntity, 
    //             media_player_entity_id = mediaPlayerEntity, 
    //             cache, message, 
    //             options
    //         } , cancellationToken);

    // public Task<HttpResponseMessage> SpeakPiper(string mediaPlayerEntity, string message, bool cache = true, PiperSettings? options = null, CancellationToken cancellationToken = default)
    //     => CallService("tts", "speak", new
    //         {
    //             entity_id = "tts.piper", 
    //             media_player_entity_id = mediaPlayerEntity, 
    //             cache, message, 
    //             options
    //         }, cancellationToken);
    // public Task<HttpResponseMessage> SpeakPiper(IEnumerable<string> mediaPlayerEntity, string message, bool cache = true, PiperSettings? options = null, CancellationToken cancellationToken = default)
    //     => CallService("tts", "speak", new
    //         {
    //             entity_id = "tts.piper", 
    //             media_player_entity_id = mediaPlayerEntity, 
    //             cache, message, 
    //             options
    //         }, cancellationToken);


    // public Task<HttpResponseMessage> SwitchTurnOff(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(SWITCH, TURN_OFF, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> SwitchTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(SWITCH, TURN_OFF, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(SWITCH, TURN_ON, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> SwitchTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(SWITCH, TURN_ON, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> SwitchToggle(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(SWITCH, TOGGLE, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> SwitchToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(SWITCH, TOGGLE, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOn(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_ON, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_ON, new { entity_id }, cancellationToken);
    
    // public Task<HttpResponseMessage> TurnOnArea(string area_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_ON, new { area_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOnAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_ON, new { area_id }, cancellationToken);
    
    // public Task<HttpResponseMessage> TurnOnByLabel(string label_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_ON, new { label_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOnByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_ON, new { label_id }, cancellationToken);    

    // public Task<HttpResponseMessage> TurnOff(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_OFF, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_OFF, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOffArea(string area_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_OFF, new { area_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOffAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_OFF, new { area_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOffByLabel(string label_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_OFF, new { label_id }, cancellationToken);

    // public Task<HttpResponseMessage> TurnOffByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TURN_OFF, new { label_id }, cancellationToken);

    // public Task<HttpResponseMessage> Toggle(string entity_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TOGGLE, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> Toggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TOGGLE, new { entity_id }, cancellationToken);

    // public Task<HttpResponseMessage> ToggleArea(string area_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TOGGLE, new { area_id }, cancellationToken);

    // public Task<HttpResponseMessage> ToggleAreas(IEnumerable<string> area_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TOGGLE, new { area_id }, cancellationToken);

    // public Task<HttpResponseMessage> ToggleByLabel(string label_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TOGGLE, new { label_id }, cancellationToken);

    // public Task<HttpResponseMessage> ToggleByLabels(IEnumerable<string> label_id, CancellationToken cancellationToken = default)
    //     => CallService(HOME_ASSISTANT, TOGGLE, new { label_id }, cancellationToken);

    // public Task<HttpResponseMessage> ZwaveJs_SetConfigParameter(object config, CancellationToken cancellationToken = default)
    //     => CallService("zwave_js", "set_config_parameter", config, cancellationToken);
}
