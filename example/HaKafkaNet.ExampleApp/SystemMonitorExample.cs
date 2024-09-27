
using System.Text;

namespace HaKafkaNet.ExampleApp;

/// <summary>
/// Documentation:  
/// https://github.com/leosperry/ha-kafka-net/wiki/System-Monitor
/// </summary>
public class SystemMonitorExample : ISystemMonitor
{
    readonly IHaApiProvider _api;
    readonly ILogger _logger;

    private bool _sendBadStateEvents = true;


    public SystemMonitorExample(IHaApiProvider api, ILogger<SystemMonitorExample> logger)
    {
        _api = api;
        _logger = logger;
    }

    public Task BadEntityStateDiscovered(BadEntityState badEntityInfo)
    {
        if (badEntityInfo.EntityId.StartsWith("event"))
        {
            return Task.CompletedTask;
        }

        if (_sendBadStateEvents)
        {
            var message = $"Bad Entity State{Environment.NewLine}{badEntityInfo.EntityId} has a state of {badEntityInfo?.State?.State ?? "null"}";

            _api.PersistentNotification(message, default);
        }
        return Task.CompletedTask;
    }

    public Task StateHandlerInitialized() => Task.CompletedTask;

    public Task UnhandledException(AutomationMetaData automationMetaData, Exception exception)
    {
        return _api.PersistentNotification($"automation of type: [{automationMetaData.UnderlyingType}] failed with [{exception.Message}]");
    }

    public Task HaNotificationUpdate(HaNotification notification, CancellationToken ct)
    {
        // to use this make sure the automation is configured in your Home Assistant Configuration
        // https://github.com/leosperry/ha-kafka-net/blob/main/infrastructure/hakafkanet.yaml
        return Task.CompletedTask;
    }

    public async Task HaStartUpShutDown(StartUpShutDownEvent evt, CancellationToken ct)
    {
        // to use this make sure the automation is configured in your Home Assistant Configuration
        // https://github.com/leosperry/ha-kafka-net/blob/main/infrastructure/hakafkanet.yaml

        _logger.LogInformation("Home Assistant {HaEvent}", evt.Event);

        // at startup, HA can report a bunch of entities as unavailabe
        // adjust the timeout for the startup time of your system
        // the author's HA starup time is about 2 minutes, so here it is set to 3

        const int three_min = 3 * 60 * 1000;
        await evt.ShutdownStartupActions(
            () => _sendBadStateEvents = false, 
            ()  => _sendBadStateEvents = true , 
            three_min, _logger);
    }

    public async Task HaApiResponse(HaServiceResponseArgs args, CancellationToken ct)
    {
        if (args.Exception is not null)
        {
            // the call to the api threw an exception
            _logger.LogCritical(args.Exception, "critical failure calling HA service");
        }
        else if (args.Response is not null)
        {
            // the call did not throw an exception, but does not indicate success
            _logger.LogError("HA service response:{response_code}, reason:{reason}, data:{data}", args.Response.StatusCode, args.Response.ReasonPhrase, args.Data);
        }
        else
        {
            _logger.LogError("should not happen");
        }

        string title = "HA Service call failed.";
        string message = $"{args.Domain}.{args.Service} was sent {args.Data}";

        if(!(args.Domain == "notify" && args.Service == "persistent_notification"))
        {
            // dont send persistent if failure was on persistent
            await _api.PersistentNotificationDetail(message, title);
        }

        if (!(args.Domain == "notify" && args.Service == "mobile_app_my_phone"))
        {
            // dont send to phone if failure was to sending to phone
            await _api.NotifyGroupOrDevice("mobile_app_my_phone", message, title);
        }
    }
}
