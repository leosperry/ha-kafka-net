
namespace HaKafkaNet;

public interface ISystemMonitor
{
    Task BadEntityStateDiscovered(IEnumerable<BadEntityState> badStates);
    Task StateHandlerInitialized();
    Task UnhandledException(AutomationMetaData automationMetaData, Exception exception);
}
