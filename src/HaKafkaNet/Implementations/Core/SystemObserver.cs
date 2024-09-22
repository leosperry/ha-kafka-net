using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface ISystemObserver
{
    bool IsInitialized{get;}
    event Action? StateHandlerInitialized;

    void InitializeMonitors(IEnumerable<ISystemMonitor> monitors);

    void OnStateHandlerInitialized();
    void OnUnhandledException(AutomationMetaData automationMetaData, Exception exception);

    void OnBadStateDiscovered(BadEntityState badState);

    /// <summary>
    /// Will be called when a Home Assistant service is called and the response is not in the 200/300 range or if the call throws an exception.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="ct"></param>
    void OnHaServiceBadResponse(HaServiceResponseArgs args, CancellationToken ct);

    void OnHaNotification(HaNotification notification, CancellationToken ct);

    void OnHaStartUpShutdown(StartUpShutDownEvent evt, CancellationToken ct);
}

public record HaServiceResponseArgs(string Domain, string Service, object Data, HttpResponseMessage? Response, Exception? Exception);

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
    internal event Action<HaServiceResponseArgs, CancellationToken>? HaApiResponse;

    public SystemObserver(ILogger<SystemObserver> logger)
    {
        _logger = logger;

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

    public void OnHaServiceBadResponse(HaServiceResponseArgs args, CancellationToken ct)
        => HaApiResponse?.Invoke(args, ct);

    public void InitializeMonitors(IEnumerable<ISystemMonitor> monitors)
    {
        foreach (var monitor in monitors)
        {
            StateHandlerInitialized += () =>    _ = WrapTask("State Handler Initialized", ()=> monitor.StateHandlerInitialized());
            UnhandledException += (meta, ex) => _ = WrapTask("Unhandled Exception", ()=> monitor.UnhandledException(meta, ex));
            BadEntityState += (state) =>        _ = WrapTask("Bad Entity State", ()=> monitor.BadEntityStateDiscovered(state));
            Notify += (note, ct) =>             _ = WrapTask("HA Notification", () => monitor.HaNotificationUpdate(note, ct));
            HaStartUpShutdown += (evt, ct) =>   _ = WrapTask("HA StartupShutDown", () => monitor.HaStartUpShutDown(evt, ct));
            HaApiResponse += (args, ct) =>      _ = WrapTask("HA API Response", () => monitor.HaApiResponse(args, ct));
        }    
    }
}
