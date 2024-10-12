using System.Diagnostics;
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
        _client = clientFactory.CreateClient("HaKafkaNet");
        _apiConfig = config;
        _observer = observer;

        _client.BaseAddress = new Uri(config.BaseUri);

        _client.DefaultRequestHeaders.CacheControl = 
            new CacheControlHeaderValue()
            {
                NoCache = true
            };

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
}
