
using System.Text;

namespace HaKafkaNet.ExampleApp;

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
    }}
