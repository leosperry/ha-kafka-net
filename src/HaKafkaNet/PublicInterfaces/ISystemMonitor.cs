
namespace HaKafkaNet;

public interface ISystemMonitor
{
    Task BadEntityStateDiscovered(IEnumerable<BadEntityState> badStates);
    Task StateHandlerInitialized();
    Task UnhandledException(AutomationMetaData automationMetaData, Exception exception);
    Task HaNotificationUpdate(HaNotification notification, CancellationToken ct) => Task.CompletedTask;
}
