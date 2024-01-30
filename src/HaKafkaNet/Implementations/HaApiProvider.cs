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

    public async Task<(HttpResponseMessage response, HaEntityState entityState)> GetEntityState(string entity_id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"/api/states/{entity_id}", cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => (response, JsonSerializer.Deserialize<HaEntityState>(response.Content.ReadAsStream())!),
            _ => (response, null!)
        };
    }

    public async Task<(HttpResponseMessage response, HaEntityState<T> entityState)> GetEntityState<T>(string entity_id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"/api/states/{entity_id}", cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => (response, JsonSerializer.Deserialize<HaEntityState<T>>(response.Content.ReadAsStream())!),
            _ => (response, null!)
        };
    }

    public Task<HttpResponseMessage> LightToggle(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_off", new {entity_id}, cancellationToken);
    }
    
    public Task<HttpResponseMessage> LightToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_off", new {entity_id}, cancellationToken);
    }

    public Task<HttpResponseMessage> LightSetBrightness(string entity_id, byte brightness, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_on", new { entity_id, brightness }, cancellationToken);
    }

    public Task<HttpResponseMessage> LightSetBrightness(IEnumerable<string> entity_id, byte brightness, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_on", new { entity_id, brightness }, cancellationToken);
    }

    public Task<HttpResponseMessage> LightTurnOff(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_off", new {entity_id}, cancellationToken);
    }

    public Task<HttpResponseMessage> LightTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_off", new {entity_id}, cancellationToken);
    }

    public Task<HttpResponseMessage> LightTurnOn(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_on", new { entity_id }, cancellationToken);
    }
    public Task<HttpResponseMessage> LightTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_on", new { entity_id }, cancellationToken);
    }
    public Task<HttpResponseMessage> LightTurnOn(LightTurnOnModel settings, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_on", settings, cancellationToken);
    }

    public Task<HttpResponseMessage> NotifyGroupOrDevice(string groupName, string message, CancellationToken cancellationToken = default)
    {
        return CallService("notify", groupName, new { message }, cancellationToken);
    }

    public Task<HttpResponseMessage> NotifyAlexaMedia(string message, string[] targets, CancellationToken cancellationToken = default)
    {
        return CallService("notify", "alexa_media", new { message, target = targets }, cancellationToken);
    }

    public Task<HttpResponseMessage> PersistentNotification(string message, CancellationToken cancellationToken = default)
    {
        return CallService("notify", "persistent_notification", new {message}, cancellationToken);
    }

    public Task<HttpResponseMessage> SwitchTurnOff(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "turn_off", new { entity_id }, cancellationToken);
    }
    public Task<HttpResponseMessage> SwitchTurnOff(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "turn_off", new { entity_id }, cancellationToken);
    }

    public Task<HttpResponseMessage> SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "turn_on", new { entity_id }, cancellationToken);
    }

    public Task<HttpResponseMessage> SwitchTurnOn(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "turn_on", new { entity_id }, cancellationToken);
    }

    public Task<HttpResponseMessage> SwitchToggle(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "toggle", new { entity_id }, cancellationToken);
    }
    public Task<HttpResponseMessage> SwitchToggle(IEnumerable<string> entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "toggle", new { entity_id }, cancellationToken);
    }
}
