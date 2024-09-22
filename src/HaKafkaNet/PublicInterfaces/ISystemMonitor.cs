
namespace HaKafkaNet;

public interface ISystemMonitor
{
    Task BadEntityStateDiscovered(BadEntityState badState);
    Task StateHandlerInitialized();
    Task UnhandledException(AutomationMetaData automationMetaData, Exception exception);
    Task HaNotificationUpdate(HaNotification notification, CancellationToken ct) => Task.CompletedTask;
    Task HaStartUpShutDown(StartUpShutDownEvent evt, CancellationToken ct) => Task.CompletedTask;
    Task HaApiResponse(HaServiceResponseArgs args, CancellationToken ct) => Task.CompletedTask;
}
