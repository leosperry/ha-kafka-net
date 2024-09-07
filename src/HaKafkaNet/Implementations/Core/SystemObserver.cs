using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface ISystemObserver
{
    bool IsInitialized{get;}
    event Action? StateHandlerInitialized;
    void OnStateHandlerInitialized();
    void OnUnhandledException(AutomationMetaData automationMetaData, Exception exception);

    void OnBadStateDiscovered(BadEntityState badState);

    void OnHaNotification(HaNotification notification, CancellationToken ct);

    void OnHaStartUpShutdown(StartUpShutDownEvent evt, CancellationToken ct);
}

/// <summary>
/// Kafka handlers should be initialized by kafka flow and not other classes via DI.
/// Therefore, we need an observer to report on thier state.
/// </summary>
internal class SystemObserver : ISystemObserver
{
    ILogger _logger;

    public bool IsInitialized { get; private set; }
    public event Action? StateHandlerInitialized;
    internal event Action<AutomationMetaData, Exception>? UnhandledException;
    internal event Action<BadEntityState>? BadEntityState;
    internal event Action<HaNotification, CancellationToken>? Notify;
    internal event Action<StartUpShutDownEvent, CancellationToken>? HaStartUpShutdown;

    public SystemObserver(IEnumerable<ISystemMonitor> monitors, ILogger<SystemObserver> logger)
    {
        _logger = logger;
        foreach (var monitor in monitors)
        {
            StateHandlerInitialized += () =>    _ = WrapTask("State Handler Initialized", ()=> monitor.StateHandlerInitialized());
            UnhandledException += (meta, ex) => _ = WrapTask("Unhandled Exception", ()=> monitor.UnhandledException(meta, ex));
            BadEntityState += (state) =>        _ = WrapTask("Bad Entity State", ()=> monitor.BadEntityStateDiscovered(state));
            Notify += (note, ct) =>             _ = WrapTask("HA Notification", () => monitor.HaNotificationUpdate(note, ct));
            HaStartUpShutdown += (evt, ct) =>   _ = WrapTask("HA StartupShutDown", () => monitor.HaStartUpShutDown(evt, ct));
        }
    }

    private async Task WrapTask(string taskType, Func<Task> funcToErrorHandle)
    {
        Task t;
        Dictionary<string, object> scope = new()
        {
            {"systemMonitorCall",DateTime.Now},
            {"systemMonitorCallType", taskType}
        };
        using(_logger.BeginScope(scope))
        {
            try
            {
                t = funcToErrorHandle();
                await t;
                t.Wait();
            }
            catch (TaskCanceledException cancelEx)
            {
                _logger.LogInformation(cancelEx, "Task Canceled in System Observer");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error in System Observer");
                //swallow this so that other handlers continue to run
            }
        }
    }

    public void OnStateHandlerInitialized()
    {
        IsInitialized = true;
        StateHandlerInitialized?.Invoke();
    }

    public void OnUnhandledException(AutomationMetaData automationMetaData, Exception exception)
        => UnhandledException?.Invoke(automationMetaData, exception);

    public void OnBadStateDiscovered(BadEntityState badState)
        => BadEntityState?.Invoke(badState);

    public void OnHaNotification(HaNotification notification, CancellationToken ct)
        => Notify?.Invoke(notification, ct);

    public void OnHaStartUpShutdown(StartUpShutDownEvent evt, CancellationToken ct)
        => HaStartUpShutdown?.Invoke(evt, ct);
}
