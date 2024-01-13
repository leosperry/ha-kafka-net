using System.Net.Http.Headers;
using System.Text.Json;

namespace HaKafkaNet;

internal class HaApiProvider : IHaApiProvider
{
    HttpClient _client;
    HaApiConfig _apiConfig;

    public HaApiProvider(IHttpClientFactory clientFactory, HaApiConfig config)
    {
        _client = clientFactory.CreateClient();
        _apiConfig = config;

        _client.BaseAddress = new Uri(config.BaseUri);
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _apiConfig.AccessToken);
    }

    public async Task<HttpResponseMessage> CallService(string domain, string service, object data, CancellationToken cancellationToken = default)
    {
        using StringContent json = new StringContent(JsonSerializer.Serialize(data));
        return await _client.PostAsync($"/api/services/{domain}/{service}",json, cancellationToken);
    }


    public Task<HttpResponseMessage> LightSetBrightness(string entity_id, byte brightness, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_on", new { entity_id, brightness }, cancellationToken);
    }

    public Task<HttpResponseMessage> LightTurnOff(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_off", new {entity_id = entity_id}, cancellationToken);
    }

    public Task<HttpResponseMessage> LightTurnOn(string entity_id, object setting = null!, CancellationToken cancellationToken = default)
    {
        return CallService("light", "turn_on", new { entity_id }, cancellationToken);
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

    public Task<HttpResponseMessage> SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "turn_on", new { entity_id }, cancellationToken);
    }

    public Task<HttpResponseMessage> SwitchToggle(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "toggle", new { entity_id }, cancellationToken);
    }
}
