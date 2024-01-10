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

    public async Task CallService(string domain, string service, object data, CancellationToken cancellationToken = default)
    {
        using StringContent json = new StringContent(JsonSerializer.Serialize(data));
        var response = await _client.PostAsync($"/api/services/{domain}/{service}",json, cancellationToken);
        System.Console.WriteLine(response.StatusCode);
    }

    public Task PersistentNotification(string message, CancellationToken cancellationToken = default)
    {
        return CallService("notify", "persistent_notification", new {message}, cancellationToken);
    }

    public Task SwitchTurnOff(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "turn_off", new { entity_id }, cancellationToken);
    }

    public Task SwitchTurnOn(string entity_id, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "turn_on", new { entity_id }, cancellationToken);
    }

    public Task LightSetBrightness(string entity_id, byte brightness, CancellationToken cancellationToken = default)
    {
        return CallService("switch", "turn_on", new { entity_id, brightness }, cancellationToken);
    }

    public Task GroupNotify(string groupName, string message, CancellationToken cancellationToken = default)
    {
        return CallService("notify", groupName, new { message }, cancellationToken);
    }

    //TODO: add common service calls such as light.turn_on
}
