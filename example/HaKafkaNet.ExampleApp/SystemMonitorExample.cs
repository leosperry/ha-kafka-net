
using System.Text;

namespace HaKafkaNet.ExampleApp;

public class SystemMonitorExample : ISystemMonitor
{
    readonly IHaApiProvider _api;

    public SystemMonitorExample(IHaApiProvider api)
    {
        _api = api;
    }

    public Task BadEntityStateDiscovered(IEnumerable<BadEntityState> badStates)
    {
        StringBuilder sb = new();
        sb.AppendLine("bad entity states");
        foreach (var item in badStates)
        {
            if (item.State is null)
            {
                sb.AppendLine($"{item.EntityId} could not be found");            
            }
            else
            {
                sb.AppendLine($"{item.EntityId} has a state of {item.State.State}");
            }
        }
        return _api.PersistentNotification(sb.ToString());
    }

    public Task StateHandlerInitialized() => Task.CompletedTask;

    public Task UnhandledException(AutomationMetaData automationMetaData, Exception exception)
    {
        return _api.PersistentNotification($"automation of type: [{automationMetaData.UnderlyingType}] failed with [{exception.Message}]");
    }
}
