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

    public async Task CallService(string domain, string service, object data)
    {
        using StringContent json = new StringContent(JsonSerializer.Serialize(data));
        var response = await _client.PostAsync($"/api/services/{domain}/{service}",json);
        System.Console.WriteLine(response.StatusCode);
    }

    public Task PersistentNotification(string message)
    {
        return CallService("notify", "persistent_notification", new {message});
    }

    public Task SwitchTurnOff(string entity_id)
    {
        return CallService("switch", "turn_off", new { entity_id });
    }

    public Task SwitchTurnOn(string entity_id)
    {
        return CallService("switch", "turn_on", new { entity_id });
    }

    public Task LightSetBrightness(string entity_id, byte brightness = 255)
    {
        return CallService("switch", "turn_on", new { entity_id, brightness });
    }

    public Task GroupNotify(string groupName, string message)
    {
        return CallService("notify", groupName, new { message });
    }

    //TODO: add common service calls such as light.turn_on
}
