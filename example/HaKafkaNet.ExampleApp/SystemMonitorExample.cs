
using System.Text;

namespace HaKafkaNet.ExampleApp;

public class SystemMonitorExample : ISystemMonitor
{
    readonly IHaApiProvider _api;

    public SystemMonitorExample(IHaApiProvider api)
    {
        _api = api;
    }

    public Task BadEntityStateDiscovered(BadEntityState badState)
    {
        string message;

        message = $"{badState.EntityId} has a state of {badState.State?.State ?? "null"}";
        
        return _api.PersistentNotification(message, default);
    }

    public Task StateHandlerInitialized() => Task.CompletedTask;

    public Task UnhandledException(AutomationMetaData automationMetaData, Exception exception)
    {
        return _api.PersistentNotification($"automation of type: [{automationMetaData.UnderlyingType}] failed with [{exception.Message}]");
    }
}
