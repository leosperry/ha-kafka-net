using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaKafkaNet;

internal class HaApiProvider : IHaApiProvider
{
    readonly HttpClient _client;
    readonly HomeAssistantConnectionInfo _apiConfig;
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
        SWITCH = "switch" ;
    #endregion

    public HaApiProvider(IHttpClientFactory clientFactory, HomeAssistantConnectionInfo config)
    {
        _client = clientFactory.CreateClient();
        _apiConfig = config;

        _client.BaseAddress = new Uri(config.BaseUri);
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _apiConfig.AccessToken);
    }

    public async Task<HttpResponseMessage> CallService(string domain, string service, object data, CancellationToken cancellationToken = default)
    {
        using StringContent json = new StringContent(JsonSerializer.Serialize(data, _options));
        return await _client.PostAsync($"/api/services/{domain}/{service}",json, cancellationToken);
    }

    public async Task<HttpResponseMessage> GetErrorLog(CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync("/api/error_log", cancellationToken);
    }


    public Task<(HttpResponseMessage response, HaEntityState? entityState)> GetEntityState(string entity_id, CancellationToken cancellationToken = default)
    {
        return GetEntity(entity_id, cancellationToken);
    }

    public async Task<(HttpResponseMessage response, HaEntityState<string, T>? entityState)> GetEntityState<T>(string entity_id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"/api/states/{entity_id}", cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => (response, JsonSerializer.Deserialize<HaEntityState<string, T>>(response.Content.ReadAsStream())!),
            _ => (response, null!)
        };
    }

    public async Task<(HttpResponseMessage response, HaEntityState? entityState)> GetEntity(string entity_id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"/api/states/{entity_id}", cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => (response, JsonSerializer.Deserialize<HaEntityState>(response.Content.ReadAsStream())!),
            _ => (response, null!)
        };
    }

    public async Task<(HttpResponseMessage response, T? entityState)> GetEntity<T>(string entity_id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"/api/states/{entity_id}", cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => (response, JsonSerializer.Deserialize<T>(response.Content.ReadAsStream())!),
            _ => (response, default(T?))
        };
    }

    public Task<HttpResponseMessage> LightToggle(string entity_id, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TOGGLE, new {entity_id}, cancellationToken);
    
    public Task<HttpResponseMessage> LightToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TOGGLE, new {entity_id}, cancellationToken);

    public Task<HttpResponseMessage> LightSetBrightness(string entity_id, byte brightness, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TURN_ON, new { entity_id, brightness }, cancellationToken);

    public Task<HttpResponseMessage> LightSetBrightness(IEnumerable<string> entity_id, byte brightness, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TURN_ON, new { entity_id, brightness }, cancellationToken);

    public Task<HttpResponseMessage> LightTurnOff(string entity_id, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TURN_OFF, new {entity_id}, cancellationToken);

    public Task<HttpResponseMessage> LightTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TURN_OFF, new {entity_id}, cancellationToken);

    public Task<HttpResponseMessage> LightTurnOn(string entity_id, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TURN_ON, new { entity_id }, cancellationToken);

    public Task<HttpResponseMessage> LightTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TURN_ON, new { entity_id }, cancellationToken);

    public Task<HttpResponseMessage> LightTurnOn(LightTurnOnModel settings, CancellationToken cancellationToken = default)
        => CallService(LIGHT, TURN_ON, settings, cancellationToken);

    public Task<HttpResponseMessage> LockLock(string entity_id, CancellationToken cancellationToken = default) 
        => CallService(LOCK, LOCK, new{entity_id}, cancellationToken);

    public Task<HttpResponseMessage> LockUnLock(string entity_id, CancellationToken cancellationToken = default)
        => CallService(LOCK, "unlock", new{entity_id}, cancellationToken);
    public Task<HttpResponseMessage> LockOpen(string entity_id, CancellationToken cancellationToken = default)
        => CallService(LOCK, "open", new{entity_id}, cancellationToken);
    public Task<HttpResponseMessage> NotifyGroupOrDevice(string groupName, string message, CancellationToken cancellationToken = default)
        => CallService(NOTIFY, groupName, new { message }, cancellationToken);

    public Task<HttpResponseMessage> NotifyAlexaMedia(string message, string[] targets, CancellationToken cancellationToken = default)
        => CallService(NOTIFY, "alexa_media", new { message, target = targets }, cancellationToken);

    public Task<HttpResponseMessage> PersistentNotification(string message, CancellationToken cancellationToken = default)
        =>  CallService(NOTIFY, "persistent_notification", new {message}, cancellationToken);

    public Task<HttpResponseMessage> RestartHomeAssistant(CancellationToken cancellationToken = default)
        => CallService(HOME_ASSISTANT, "restart", new{}, cancellationToken);

    public Task<HttpResponseMessage> RemoteSendCommand(string entity_id, string command, CancellationToken cancellationToken = default)
        => CallService("remote", "send_command", new{ entity_id,  command }, cancellationToken);

    public Task<HttpResponseMessage> SwitchTurnOff(string entity_id, CancellationToken cancellationToken = default)
        => CallService(SWITCH, TURN_OFF, new { entity_id }, cancellationToken);

    public Task<HttpResponseMessage> SwitchTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(SWITCH, TURN_OFF, new { entity_id }, cancellationToken);

    public Task<HttpResponseMessage> SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default)
        => CallService(SWITCH, TURN_ON, new { entity_id }, cancellationToken);

    public Task<HttpResponseMessage> SwitchTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(SWITCH, TURN_ON, new { entity_id }, cancellationToken);

    public Task<HttpResponseMessage> SwitchToggle(string entity_id, CancellationToken cancellationToken = default)
        => CallService(SWITCH, TOGGLE, new { entity_id }, cancellationToken);

    public Task<HttpResponseMessage> SwitchToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(SWITCH, TOGGLE, new { entity_id }, cancellationToken);

    public Task<HttpResponseMessage> TurnOn(string entity_id, CancellationToken cancellationToken = default) 
        => CallService(HOME_ASSISTANT, TURN_ON, new {entity_id}, cancellationToken);

    public Task<HttpResponseMessage> TurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(HOME_ASSISTANT, TURN_ON, new {entity_id}, cancellationToken);

    public Task<HttpResponseMessage> TurnOff(string entity_id, CancellationToken cancellationToken = default)
        => CallService(HOME_ASSISTANT, TURN_OFF, new {entity_id}, cancellationToken);

    public Task<HttpResponseMessage> TurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(HOME_ASSISTANT, TURN_OFF, new {entity_id}, cancellationToken);

    public Task<HttpResponseMessage> Toggle(string entity_id, CancellationToken cancellationToken = default)
        => CallService(HOME_ASSISTANT, TOGGLE, new {entity_id}, cancellationToken);

    public Task<HttpResponseMessage> Toggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
        => CallService(HOME_ASSISTANT, TOGGLE, new {entity_id}, cancellationToken);
    
    public Task<HttpResponseMessage> ZwaveJs_SetConfigParameter(object config, CancellationToken cancellationToken = default)
        => CallService("zwave_js", "set_config_parameter", config, cancellationToken);
}
