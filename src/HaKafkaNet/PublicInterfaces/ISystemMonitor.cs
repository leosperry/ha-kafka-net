
namespace HaKafkaNet;

/// <summary>
/// Gives information about events in the system
/// Also gives information about automation execution from a global context
/// </summary>
public interface ISystemMonitor
{
    /// <summary>
    /// called when entities enter unknown or unavailable states
    /// </summary>
    /// <param name="badState"></param>
    /// <returns></returns>
    Task BadEntityStateDiscovered(BadEntityState badState);

    /// <summary>
    /// Called once at startup after HaKafkaNet initializes, and immediately 
    /// before states begin being handled
    /// </summary>
    /// <returns></returns>
    Task StateHandlerInitialized();

    /// <summary>
    /// called when an automation does not handle an exception
    /// </summary>
    /// <param name="automationMetaData"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    Task UnhandledException(AutomationMetaData automationMetaData, Exception exception);

    /// <summary>
    /// Called when a notification in HA is updated
    /// requires configuration in HA
    /// see: https://github.com/leosperry/ha-kafka-net/blob/main/infrastructure/hakafkanet.yaml
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task HaNotificationUpdate(HaNotification notification, CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Called when HA is starting up or shutting down
    /// requires configuration in HA
    /// see: https://github.com/leosperry/ha-kafka-net/blob/main/infrastructure/hakafkanet.yaml
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task HaStartUpShutDown(StartUpShutDownEvent evt, CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// called when HA returns a non-200 response
    /// </summary>
    /// <param name="args"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task HaApiResponse(HaServiceResponseArgs args, CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Called when any errors occur during initialization
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    Task InitializationFailure(InitializationError[] errors) =>  Task.Run(() => ServicesExtensions.TrySendNotification(errors));

    /// <summary>
    /// For strongly typed automations, this is called when type conversion fails
    /// </summary>
    /// <param name="auto"></param>
    /// <param name="sc"></param>
    /// <param name="ex"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task AutomationTypeConversionFailure(IAutomationBase auto, HaEntityStateChange sc, Exception ex, CancellationToken ct) => Task.CompletedTask;
}